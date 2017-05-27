using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.server.commands;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.server.messages;
using gtmp.evilempire.server.services;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace gtmp.evilempire.server
{
    class ServerRealm : IDisposable
    {
        readonly object _syncRoot = new object();

        CancellationTokenSource heartbeatCancellationTokenSource;
        Thread heartbeatThread;

        API api;
        ServiceContainer services;
        IClientService clients;
        ISessionService sessions;
        ICharacterService characters;
        ISerializationService serialization;
        ISessionStateTransitionService sessionStateTransition;
        ICommandService commands;

        readonly IDictionary<string, MessageHandlerBase> clientMessageHandlers;

        public ServerRealm(API api)
        {
            if (api == null)
            {
                throw new ArgumentNullException(nameof(api));
            }

            this.api = api;

            var map = MapLoader.LoadFrom("maps");
            ServerMapLoader.Load(map, api);

            services = ServiceContainer.Create();
            services.Register<IPlatformService>(new GtmpPlatformService(api));
            services.Register(map);

            clients = services.Get<IClientService>();
            sessions = services.Get<ISessionService>();
            serialization = services.Get<ISerializationService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();
            commands = services.Get<ICommandService>();
            characters = services.Get<ICharacterService>();

            clientMessageHandlers = GetClientMessageHandlers(services);
            AddPlatformHooks(api);
            AddChatCommands();
            BeginHeartbeat();
        }

        public void Dispose()
        {
            if (api != null)
            {
                api.onPlayerFinishedDownload -= OnPlayerFinishedDownload;
                api.onClientEventTrigger -= OnClientEventTrigger;
            }
            api = null;

            heartbeatCancellationTokenSource?.Cancel();
            heartbeatThread?.Join();

            heartbeatCancellationTokenSource?.Dispose();
            heartbeatCancellationTokenSource = null;

            heartbeatThread = null;

            services?.Dispose();
            services = null;
        }

        void AddPlatformHooks(API api)
        {
            api.onPlayerFinishedDownload += OnPlayerFinishedDownload;
            api.onClientEventTrigger += OnClientEventTrigger;
            api.onPlayerDisconnected += OnPlayerDisconnected;
            api.onChatCommand += OnChatCommand;
        }

        void AddChatCommands()
        {
            var commandTypes = typeof(ServerRealm).Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(Command)));
            foreach (var commandType in commandTypes)
            {
                var command = Activator.CreateInstance(commandType, new[] { services }) as Command;
                var commandInfo = command?.Info;
                commands.RegisterCommand(commandInfo);
            }
        }

        void OnPlayerFinishedDownload(Client client)
        {
            var managedClient = clients.CreateFromPlatformObject(client);
            clients.RegisterTuple(managedClient, client);

            var session = sessions.CreateSession(managedClient);
            sessionStateTransition.Transit(session, SessionState.Connected);
        }

        void OnPlayerDisconnected(Client client, string reason)
        {
            var managedClient = clients.FindByPlatformObject(client);
            var session = sessions.GetSession(managedClient);
            if (session.Character != null)
            {
                characters.UpdatePosition(session.Character.Id, session.Client.Position, session.Client.Rotation);
            }
            sessions.RemoveSession(session);
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

        void OnChatCommand(Client client, string command, CancelEventArgs e)
        {
            var managedClient = clients.FindByPlatformObject(client);
            var session = sessions.GetSession(managedClient);

            var result = commands.ExecuteCommand(session, command);

            e.Cancel = !result.Success;
            e.Reason = result.ResponseMessage;
        }

        void BeginHeartbeat()
        {
            lock (_syncRoot)
            {
                if (heartbeatThread != null)
                {
                    throw new InvalidOperationException("Can not start heartbeat twice.");
                }
                heartbeatCancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => this.Heartbeat(heartbeatCancellationTokenSource.Token));
                thread.Start();
            }
        }

        void Heartbeat(CancellationToken cancellationToken)
        {
            const int maxDirtyCounts = 200;
            int dirtyCounts = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (dirtyCounts++ > maxDirtyCounts)
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    sessions.RemoveStaleSessions();

                    sessions.StoreSessionState();

                    dirtyCounts = 0;
                    sw.Stop();
                    using (ConsoleColor.Cyan.Foreground())
                    {
                        Console.WriteLine($"Heartbeat completed within {sw.ElapsedMilliseconds}ms.");
                    }
                }
                Thread.Sleep(100);
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
