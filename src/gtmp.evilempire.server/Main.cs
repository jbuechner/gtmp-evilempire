using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared.Math;
using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server
{
    public class Main : Script
    {
        public ServiceContainer Services { get; private set; }

        public Map Map { get; private set; }

        IDictionary<string, ClientEventCallback> ClientEventCallbacks { get; } = new Dictionary<string, ClientEventCallback> {
            { "login", ((ClientEventCallbackWithResponse)OnClientLogin).WrapIntoFailsafeResponse("login:response") }
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
                var loadingPoint = Map.GetPoint(MapPointType.Loading, 0)?.Position ?? new Vector3(-500, -500, 0);

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
                dynamic args = null;
                if (arguments != null && arguments.Length == 1 && arguments[0] is string)
                {
                    var jsonSerializer = Services.Get<IJsonSerializer>();
                    args = jsonSerializer.Parse(arguments[0].ToString());
                }
                eventCallback(Services, sender, args);
            }
        }

        static IServiceResult OnClientLogin(ServiceContainer services, Client client, dynamic args) // todo: less boilerplate request processing
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
            if (args.credentials != null)
            {
                args = args.credentials;
            }
            string username = args.username;
            string password = args.password;
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
                    var startingPoint = map.GetPoint(MapPointType.Teleport, 0);
                    if (startingPoint != null)
                    {
                        client.position = startingPoint.Position;
                    }
                    client.dimension = 0;
                    client.freeze(false);
                }
            }
            return result;
        }
    }
}