using GrandTheftMultiplayer.Server.API;
using System;
using System.Linq;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;

namespace gtmp.evilempire.server
{
    class GtmpMainScript : Script
    {
        readonly object _syncRoot = new object();

        ServerRealm serverRealm;

        public GtmpMainScript()
        {
#if DEBUG
            if (Environment.GetCommandLineArgs().Any(p => string.CompareOrdinal(p, "--dbgbreak") == 0))
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif

            API.onResourceStart += OnResourceStart;
            API.onResourceStop += OnResourceStop;
        }

        void OnResourceStop()
        {
            lock(_syncRoot)
            {
                serverRealm?.Dispose();
                serverRealm = null;
            }
        }

        void OnResourceStart()
        {
            lock(_syncRoot)
            {
                API.setCommandErrorMessage(string.Empty);
                serverRealm = new ServerRealm(API);
            }
        }

        
    }
}
