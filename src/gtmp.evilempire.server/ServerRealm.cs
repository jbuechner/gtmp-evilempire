using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.server.messages;
using gtmp.evilempire.server.services;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gtmp.evilempire.server
{
    class ServerRealm : IDisposable
    {
        API api;
        ServiceContainer services;
        IClientService clients;
        ISessionService sessions;
        ISerializationService serialization;
        ISessionStateTransitionService sessionStateTransition;

        readonly IDictionary<string, MessageHandlerBase> clientMessageHandlers;

        public ServerRealm(API api)
        {
            if (api == null)
            {
                throw new ArgumentNullException(nameof(api));
            }

            this.api = api;

            services = ServiceContainer.Create();
            services.Register<IPlatformService>(new GtmpPlatformService(api));

            clients = services.Get<IClientService>();
            sessions = services.Get<ISessionService>();
            serialization = services.Get<ISerializationService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();

            clientMessageHandlers = GetClientMessageHandlers(services);
            AddPlatformHooks(api);
        }

        public void Dispose()
        {
            if (api != null)
            {
                api.onPlayerFinishedDownload -= OnPlayerFinishedDownload;
                api.onClientEventTrigger -= OnClientEventTrigger;
            }
            api = null;

            services?.Dispose();
            services = null;
        }

        void AddPlatformHooks(API api)
        {
            api.onPlayerFinishedDownload += OnPlayerFinishedDownload;
            api.onClientEventTrigger += OnClientEventTrigger;
        }

        void OnPlayerFinishedDownload(Client client)
        {
            var managedClient = clients.CreateFromPlatformObject(client);
            clients.RegisterTuple(managedClient, client);

            var session = sessions.CreateSession(managedClient);
            sessionStateTransition.Transit(session, SessionState.Connected);
        }

        void OnClientEventTrigger(Client client, string eventName, params object[] args)
        {
            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] is string)
                    {
                        args[i] = serialization.DeserializeFromDesignatedJson(args[i].AsString());
                    }
                }

                if (args.Length == 1)
                {
                    if (args[0] is IEnumerable<object>)
                    {
                        args = ((IEnumerable<object>)args[0]).ToArray();
                    }
                }
            }
            
            var managedClient = clients.FindByPlatformObject(client);
            var session = sessions.GetSession(managedClient);
            MessageHandlerBase handler;
            if (clientMessageHandlers.TryGetValue(eventName, out handler))
            {
                handler?.ProcessClientMessage(session, args);
            }
        }

        static IDictionary<string, MessageHandlerBase> GetClientMessageHandlers(ServiceContainer services)
        {
            IDictionary<string, MessageHandlerBase> result = new Dictionary<string, MessageHandlerBase>();
            var messageHandlerTypes = typeof(ServerRealm).Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(MessageHandlerBase)));
            foreach (var messageHandlerType in messageHandlerTypes)
            {
                var messageHandler = (MessageHandlerBase)Activator.CreateInstance(messageHandlerType, services);
                result.Add(messageHandler.EventName, messageHandler);
            }
            return result;
        }
    }
}
