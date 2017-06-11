using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.db;
using gtmp.evilempire.entities;
using gtmp.evilempire.ipc;
using gtmp.evilempire.server.commands;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.server.messages;
using gtmp.evilempire.server.services;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Map map;
        ServiceContainer services;
        IPlatformService platform;
        IClientService clients;
        ISessionService sessions;
        ICharacterService characters;
        ISerializationService serialization;
        ISessionStateTransitionService sessionStateTransition;
        ICommandService commands;
        IpcServer ipc;

        readonly ServerTimerRealm timers;
        readonly IDictionary<string, MessageHandlerBase> clientMessageHandlers;

        public ServerRealm(API api)
        {
            if (api == null)
            {
                throw new ArgumentNullException(nameof(api));
            }

            this.api = api;
            ipc = new IpcServer();
            UpdateServerStatus();

            services = CreateServiceContainer();
            services.Register<IPlatformService>(platform = new GtmpPlatformService(api));

            map = MapLoader.LoadFrom("maps");
            ServerMapLoader.Load(map, platform, api);
            services.Register(map);

            timers = new ServerTimerRealm(services);

            clients = services.Get<IClientService>();
            sessions = services.Get<ISessionService>();
            serialization = services.Get<ISerializationService>();
            commands = services.Get<ICommandService>();
            characters = services.Get<ICharacterService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();

            clientMessageHandlers = GetClientMessageHandlers(services);
            AddPlatformHooks(api);
            AddChatCommands();
            AddSessionStateTransitions();
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

            ipc?.Dispose();
            ipc = null;
        }

        static ServiceContainer CreateServiceContainer()
        {
            var services = new ServiceContainer();
            services.Register<ServiceContainer>(services);
            services.Register<ISerializationService, SerializationService>();
            services.Register<IDbService>(new DbService(evilempire.Constants.Database.DatabasePath));
            services.Register<IItemService, ItemService>();
            services.Register<ISessionService, SessionService>();
            services.Register<IClientService, ClientService>();
            services.Register<IAuthenticationService, AuthenticationService>();
            services.Register<ICharacterService, CharacterService>();
            services.Register<ICommandService, CommandService>();
            services.Register<ISessionStateTransitionService, SessionStateTransitionService>();
            services.Register<IVehicleService, VehicleService>();

            return services;
        }

        void AddPlatformHooks(API api)
        {
            api.onPlayerConnected += OnPlayerConnected;
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

        void AddSessionStateTransitions()
        {
            var sessionStateHandlerTypes = typeof(ServerRealm).Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(SessionStateHandlerBase)));
            if (sessionStateHandlerTypes != null)
            {
                foreach (var sessionStateHandlerType in sessionStateHandlerTypes)
                {
                    var sessionStateHandler = (SessionStateHandlerBase)Activator.CreateInstance(sessionStateHandlerType, new[] { services });
                    sessionStateTransition.RegisterTransition(sessionStateHandler);
                }
            }
        }

        void OnPlayerConnected(Client client)
        {
            var managedClient = clients.CreateFromPlatformObject(client);
            clients.RegisterTuple(managedClient, client);

            var session = sessions.CreateSession(managedClient);
            sessionStateTransition.Transit(session, SessionState.Connected);
            UpdateServerStatus();
        }

        void OnPlayerFinishedDownload(Client client)
        {
            var managedClient = clients.FindByPlatformObject(client);
            if (managedClient == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"Client is ready but no managed client object found for {client.name} / {client.address}. Disconnecting client.");
                }
                client.kick("Internal Server Error");
            }
            var session = sessions.GetSession(managedClient);
            if (session == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"Client is ready but no session found for {client.name} / {client.address}. Disconnecting client.");
                }
                client.kick("Internal Server Error");
            }
            sessionStateTransition.Transit(session, SessionState.Ready);
        }

        void OnPlayerDisconnected(Client client, string reason)
        {
            var managedClient = clients.FindByPlatformObject(client);
            if (managedClient == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"Client disconnected but found not managed client object for {client.name} / {client.address}");
                }
                return;
            }

            var session = sessions.GetSession(managedClient);
            if (session == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"Client disconnected but found not session for {client.name} / {client.address}");
                }
                return;
            }

            if (session.Character != null && session.UpdateDatabasePosition)
            {
                characters.UpdatePosition(session.Character.Id, session.Client.Position, session.Client.Rotation);
            }
            sessions.RemoveSession(session);
            UpdateServerStatus();
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

            if (session == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"Got a client message but found not session for {client.name} / {client.address}");
                }
                return;
            }

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
            const int sleepTime = 10;
            const int maxDirtyCounts = 10000 / sleepTime;
            int dirtyCounts = 0;

            Stopwatch sw = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested)
            {
                var delta = sw.ElapsedMilliseconds;
                sw.Restart();
                timers.Process(cancellationToken, delta);

                if (dirtyCounts++ > maxDirtyCounts)
                {
                    Stopwatch laundryRuntime = new Stopwatch();
                    laundryRuntime.Start();

                    sessions.RemoveStaleSessions();

                    sessions.StoreSessionState();

                    dirtyCounts = 0;
                    laundryRuntime.Stop();
                    using (ConsoleColor.Cyan.Foreground())
                    {
                        Console.WriteLine($"Heartbeat completed within {laundryRuntime.ElapsedMilliseconds}ms.");
                    }
                }
                Thread.Sleep(sleepTime);
            }
            sw.Stop();
        }

        void UpdateServerStatus()
        {
            var status = new ServerStatus
            {
                Version = "1.0",
                MaximumNumbersOfPlayers = api.getMaxPlayers(),
                CurrentNumberOfPlayers = api.getAllPlayers()?.Count ?? 0
            };
            ipc.UpdateStatus(status);
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
