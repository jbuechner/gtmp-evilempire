using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared.Math;
using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Threading.Tasks;

namespace gtmp.evilempire.server
{
    public class Main : Script
    {
        public Main()
        {
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

        }
    }
}