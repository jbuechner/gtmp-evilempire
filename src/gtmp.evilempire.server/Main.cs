using System.Security.Policy;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared.Math;

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
            this.API.onPlayerConnected += client =>
            {
                client.freezePosition = true;
                client.dimension = 1000;
                client.position = new Vector3(-500, -500, 0);
                client.stopAnimation();
            };
        }
    }
}