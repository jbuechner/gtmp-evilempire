using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;
using Newtonsoft.Json;

namespace gtmp.evilempire.server
{
    public class Main : Script
    {
        public ServiceContainer Services { get; private set; }

        public Map Map { get; private set; }

        IDictionary<string, ClientEventCallback> ClientEventCallbacks { get; } = new Dictionary<string, ClientEventCallback> {
            { "req:login", ((ClientEventCallbackWithResponse)OnClientLogin).WrapIntoFailsafeResponse("res:login") }
        };

        public Main()
        {
#if DEBUG
            if (Environment.GetCommandLineArgs().Any(p => string.CompareOrdinal(p, "--dbgbreak") == 0))
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif

            API.onResourceStart += OnResourceStart;
            API.onResourceStop += OnResourceStop;
        }

        void OnResourceStop()
        {
            Services?.Dispose();
            Services = null;
        }

        void OnResourceStart()
        {
            Map = MapLoader.LoadFrom("maps");
            ServerMapLoader.Load(Map, API);

            Services = ServiceContainer.Create();
            Services.Register(Map);

            API.onClientEventTrigger += OnClientEventTrigger;
            API.onPlayerConnected += OnPlayerConnected;
        }

        void OnPlayerConnected(Client client)
        {
            var clientService = Services.Get<IClientService>();
            var clientLifecycleService = Services.Get<IClientLifecycleService>();

            var managedClient = clientService.CreateFromPlatformObject(client);
            clientService.RegisterTuple(managedClient, client);

            clientLifecycleService.OnClientConnect(managedClient);
        }

        void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            var clientService = Services.Get<IClientService>();
            var managedClient = clientService.FindByPlatformObject(sender);

            ClientEventCallback eventCallback = null;
            if (ClientEventCallbacks.TryGetValue(eventName, out eventCallback) && eventCallback != null)
            {
                SanitizeClientArguments(arguments);
                eventCallback(Services, managedClient, arguments);
            }
            else
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"Received unknown event {eventName} from client {sender.name}.");
                }
            }
        }

        static void SanitizeClientArguments(params object[] arguments)
        {
            if (arguments != null)
            {
                for (var i = 0; i < arguments.Length; i++)
                {
                    var arg = arguments[i];
                    if (arg != null && arg is string)
                    {
                        var argAsString = (string)arg;
                        if (argAsString.StartsWith(Constants.DataSerialization.ClientServerJsonPrefix, StringComparison.Ordinal))
                        {
                            arguments[i] = JsonConvert.DeserializeObject(argAsString);
                        }
                    }
                }
            }
        }

        static IServiceResult OnClientLogin(ServiceContainer services, IClient client, params object[] args) // todo: less boilerplate request processing
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            var username = args.ElementAtOrDefault(0).AsString();
            var password = args.ElementAtOrDefault(1).AsString();
            if (username == null)
            {
                throw new ArgumentOutOfRangeException(nameof(args), "username missing");
            }
            if (password == null)
            {
                throw new ArgumentOutOfRangeException(nameof(args), "password missing");
            }
            if (!(username is string))
            {
                throw new ArgumentOutOfRangeException(nameof(args), "username is not a string");
            }
            if (!(password is string))
            {
                throw new ArgumentOutOfRangeException(nameof(args), "password is not a string");
            }
            var authorizationService = services.Get<IAuthorizationService>();
            var result = authorizationService.Authenticate(username as string, password as string);
            if (result.State == ServiceResultState.Success)
            {
                var clientService = services.Get<IClientService>();
                var loginService = services.Get<ILoginService>();
                result = loginService.Login(username as string, client);
                if (result.State == ServiceResultState.Success)
                {
                    var clientLifecycleService = services.Get<IClientLifecycleService>();
                    clientLifecycleService.OnClientLoggedIn(client);
                }
            }
            return result;
        }
    }
}