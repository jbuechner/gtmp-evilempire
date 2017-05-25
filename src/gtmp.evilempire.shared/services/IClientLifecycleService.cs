using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IClientLifecycleService
    {
        void OnClientConnect(IClient client);
        void OnClientDisconnect(IClient client);
        void OnClientLoggedIn(IClient client);
    }
}
