using GrandTheftMultiplayer.Server.API;

namespace gtmp.evilempire.server
{
    class GtmpMainScript : Script
    {
        readonly object _syncRoot = new object();

        ServerRealm serverRealm;

        public GtmpMainScript()
        {
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
