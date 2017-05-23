using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared.Math;
using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;
using static System.FormattableString;
using System.Threading.Tasks;
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
            Map = MapLoader.LoadFrom("maps");
            ServerMapLoader.Load(Map, API);

            this.API.onResourceStart += this.OnResourceStart;
            this.API.onResourceStop += this.OnResourceStop;
        }

        void OnResourceStop()
        {
            Services?.Dispose();
            Services = null;
        }

        void OnResourceStart()
        {
            Services = ServiceContainer.Create();
            Services.Register(Map);

            this.API.onClientEventTrigger += this.OnClientEventTrigger;
            this.API.onPlayerConnected += client =>
            {
                var loadingPoint = Map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0)?.Position ?? new Vector3(0, 0, 0);
                client.dimension = 1000;
                client.freeze(false);
                client.position = loadingPoint;
                client.stopAnimation();
            };
        }

        void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            ClientEventCallback eventCallback = null;
            if (ClientEventCallbacks.TryGetValue(eventName, out eventCallback) && eventCallback != null)
            {
                SanitizeClientArguments(arguments);
                eventCallback(Services, sender, arguments);
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

        static IServiceResult OnClientLogin(ServiceContainer services, Client client, params object[] args) // todo: less boilerplate request processing
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
                var loginService = services.Get<ILoginService>();
                result = loginService.Login(username as string, new PlatformClient(client));
                if (result.State == ServiceResultState.Success)
                {
                    var map = services.Get<Map>();
                    var startingPoint = map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0);
                    Console.WriteLine(Invariant($"Position player {client.name} at {startingPoint.Position}"));
                    client.dimension = 0;
                    if (startingPoint != null)
                    {
                        client.position = startingPoint.Position;
                    }
                    client.setData("cash", (double)1293481.43);
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        while (!client.IsNull)
                        {
                            double m = (double)client.getData("cash");
                            client.setData("cash", m + 200);
                            client.triggerEvent("update", "cash", m);
                            System.Threading.Thread.Sleep(1000);
                        }
                    });
                    client.freeze(false);
                }
            }
            return result;
        }
    }
}