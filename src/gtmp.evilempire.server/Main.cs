using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;
using Newtonsoft.Json;
using System.Threading;
using gtmp.evilempire.server.commands;

namespace gtmp.evilempire.server
{
    public class Main : Script
    {
        static readonly bool SavePositionsDuringHeartbeat = true;

        readonly object _syncRoot = new object();

        public ServiceContainer Services { get; private set; }

        public Map Map { get; private set; }

        IDictionary<string, ClientEventCallback> ClientEventCallbacks { get; } = new Dictionary<string, ClientEventCallback> {
            { "req:login", ((ClientEventCallbackWithResponse)OnClientLogin).WrapIntoFailsafeResponse("res:login") }
        };

        CancellationTokenSource HeartbeatCancellationTokenSource { get; set; }
        Thread HeartbeatThread { get; set; }

        public Main()
        {
#if DEBUG
            if (Environment.GetCommandLineArgs().Any(p => string.CompareOrdinal(p, "--dbgbreak") == 0))
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
            API.onChatCommand += OnChatCommand;
            API.onResourceStart += OnResourceStart;
            API.onResourceStop += OnResourceStop;
        }

        void RunHeartbeat()
        {
            lock(_syncRoot)
            {
                if (HeartbeatThread != null)
                {
                    throw new InvalidOperationException("Can not start heartbeat twice.");
                }
                HeartbeatCancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => this.Heartbeat(HeartbeatCancellationTokenSource.Token));
                thread.Start();
            }
        }

        void StopHeartbeat()
        {
            HeartbeatCancellationTokenSource.Cancel();
            HeartbeatThread.Join();
            HeartbeatThread = null;
            HeartbeatCancellationTokenSource?.Dispose();
            HeartbeatCancellationTokenSource = null;
        }

        void Heartbeat(CancellationToken cancellationToken)
        {
            var loginService = Services.Get<ILoginService>();
            var characterService = Services.Get<ICharacterService>();

            const int maxDirtyCounts = 200;
            int dirtyCounts = 0;
            DateTime lastLoggedInClientsRetreived = DateTime.MinValue;
            IList<IClient> loggedInClients = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (dirtyCounts++ > maxDirtyCounts)
                {
                    loginService.Purge();

                    if (SavePositionsDuringHeartbeat)
                    {
                        if (loginService.LastLoggedInClientsChangeTime != lastLoggedInClientsRetreived)
                        {
                            loggedInClients = loginService.GetLoggedInClients();
                        }
                        if (loggedInClients != null)
                        {
                            foreach (var loggedInClient in loggedInClients)
                            {
                                if (loggedInClient == null || !loggedInClient.IsConnected)
                                {
                                    continue;
                                }
                                var position = loggedInClient.Position;
                                var rotation = loggedInClient.Rotation;
                                characterService.UpdatePosition(loggedInClient.CharacterId, position, rotation);
                            }
                        }
                    }
                    dirtyCounts = 0;
                }
                Thread.Sleep(100);
            }
        }

        void OnResourceStop()
        {
            StopHeartbeat();
            Services?.Dispose();
            Services = null;
        }

        void OnResourceStart()
        {
            Map = MapLoader.LoadFrom("maps");
            ServerMapLoader.Load(Map, API);

            Services = ServiceContainer.Create();
            Services.Register(Map);

            StartMapRoutes();

            RegisterCommands();

            RunHeartbeat();

            API.setCommandErrorMessage(string.Empty);
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;
        }

        void StartMapRoutes()
        {
            var mapObjectService = Services.Get<MapObjectService>();
            mapObjectService.StartAllRoutes();
        }

        void RegisterCommands()
        {
            var commandService = Services.Get<ICommandService>();
            var commandTypes = typeof(Main).Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(Command)));

            foreach(var commandType in commandTypes)
            {
                var command = Activator.CreateInstance(commandType, new[] { Services }) as Command;
                var commandInfo = command?.Info;
                commandService.RegisterCommand(commandInfo);
            }
        }

        void OnPlayerConnected(Client client)
        {
            var clientService = Services.Get<IClientService>();
            var clientLifecycleService = Services.Get<IClientLifecycleService>();

            var managedClient = clientService.CreateFromPlatformObject(client);
            clientService.RegisterTuple(managedClient, client);

            clientLifecycleService.OnClientConnect(managedClient);
        }

        void OnChatCommand(Client client, string command, CancelEventArgs e)
        {
            var clientService = Services.Get<IClientService>();
            var managedClient = clientService.FindByPlatformObject(client);
            var commandService = Services.Get<ICommandService>();
            var result = commandService.ExecuteCommand(managedClient, command);
            if (result.State == ServiceResultState.Error)
            {
                e.Reason = result.Data?.AsString();
                e.Cancel = true;
            }
        }

        void OnPlayerDisconnected(Client client, string reason)
        {
            var clientService = Services.Get<IClientService>();
            var clientLifecycleService = Services.Get<IClientLifecycleService>();

            var managedClient = clientService.FindByPlatformObject(client);
            clientLifecycleService.OnClientDisconnect(managedClient);
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