using GrandTheftMultiplayer.Shared.Math;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.FormattableString;

namespace gtmp.evilempire.server.services
{
    class ClientLifecycleService : IClientLifecycleService
    {
        Map Map { get; set; }

        public ClientLifecycleService(Map map)
        {
            Map = map;
        }

        public void OnClientConnect(IClient client)
        {
            var loadingPoint = Map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0)?.Position ?? new Vector3(0, 0, 0);
            client.IsNametagVisible = false;
            client.Dimension = 1000;
            client.CanMove = false;
            client.Position = loadingPoint;
            client.StopAnimation();
        }

        public void OnClientLoggedIn(IClient client)
        {
            var startingPoint = Map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0);
            using (ConsoleColor.Cyan.Foreground())
            {
                Console.WriteLine($"{client.Name} logged in using character id {client.CharacterId}");
                Console.WriteLine(Invariant($"Position player {client.Name} at {startingPoint.Position}"));
            }

            client.Dimension = 0;
            if (startingPoint != null)
            {
                client.Position = startingPoint.Position;
            }

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
    }
}
