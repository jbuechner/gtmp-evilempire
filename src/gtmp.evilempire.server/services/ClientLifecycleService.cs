using gtmp.evilempire.entities;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using System;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.services
{
    class ClientLifecycleService : IClientLifecycleService
    {
        IDbService DbService { get; }
        ILoginService LoginService { get; }
        ICharacterService CharacterService { get; }
        Map Map { get; set; }

        public ClientLifecycleService(IDbService dbService, ILoginService loginService, ICharacterService characterService, Map map)
        {
            DbService = dbService;
            LoginService = loginService;
            CharacterService = characterService;
            Map = map;
        }

        public void OnClientConnect(IClient client)
        {
            var loadingPoint = Map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0)?.Position ?? Vector3f.One;
            client.IsNametagVisible = false;
            client.Dimension = 1000;
            client.CanMove = false;
            client.Position = loadingPoint;
            client.StopAnimation();
        }

        public void OnClientDisconnect(IClient client)
        {
            if (LoginService.IsLoggedIn(client))
            {
                LoginService.Logout(client);
                CharacterService.UpdatePosition(client.CharacterId, client.Position, client.Rotation);
            }
        }

        public void OnClientLoggedIn(IClient client)
        {
            using (ConsoleColor.Cyan.Foreground())
            {
                Console.WriteLine($"{client.Name} logged in using character id {client.CharacterId} [{client.UserGroup}].");
            }

            client.Dimension = 0;
            UpdateClientPositionWithLastKnownPosition(client);

            //todo: remove example update
            client.SetData("cash", (double)1293481.43);
            Task.Delay(1000).ContinueWith(t =>
            {
                while (client.IsConnected)
                {
                    double m = (double)client.GetData("cash");
                    client.SetData("cash", m + 200);
                    client.TriggerClientEvent("update", "cash", m);
                    System.Threading.Thread.Sleep(1000);
                }
            });
            client.CanMove = true;
        }

        void UpdateClientPositionWithLastKnownPosition(IClient client)
        {
            var character = CharacterService.GetActiveCharacter(client);

            if (character.Position.HasValue)
            {
                var vector = character.Position.Value;
                vector.Z += 0.2f;
                client.Position = vector;
            }
            else
            {
                var startingPoint = Map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0);
                if (startingPoint != null)
                {
                    client.Position = startingPoint.Position;
                }
            }

            if (character.Rotation.HasValue)
            {
                client.Rotation = character.Rotation.Value;
            }
        }
    }
}
