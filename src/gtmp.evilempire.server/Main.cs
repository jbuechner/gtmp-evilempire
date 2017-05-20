using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared.Math;
using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Collections.Generic;
using gtmp.evilempire.services;

namespace gtmp.evilempire.server
{
    public class Main : Script
    {
        public ServiceContainer Services { get; } = ServiceContainer.Create();

        IDictionary<string, ClientEventCallback> ClientEventCallbacks { get; } = new Dictionary<string, ClientEventCallback> {
            { "login", ((ClientEventCallbackWithResponse)OnClientLogin).WrapIntoFailSafeResponse("login:response") }
        };

        public Main()
        {
            this.Services.Register<IJsonSerializer>(new JsonSerializer());
            this.API.onResourceStart += this.OnResourceStart;
        }

        void OnResourceStart()
        {
            this.API.onClientEventTrigger += this.OnClientEventTrigger;
            this.API.onPlayerConnected += client =>
            {
                client.dimension = 1000;
                client.position = new Vector3(-500, -500, 0);
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

        static IServiceResult OnClientLogin(ServiceContainer services, Client client, dynamic args)
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
                throw new ArgumentOutOfRangeException(nameof(username), "username is not a string");
            }
            if (!(password is string))
            {
                throw new ArgumentOutOfRangeException(nameof(password), "password is not a string");
            }
            var authorizationService = services.Get<IAuthorizationService>();
            return authorizationService.Authenticate(username as string, password as string);
        }
    }
}