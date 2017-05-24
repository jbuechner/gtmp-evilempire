using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.services
{
    class ClientService : IClientService
    {
        readonly ConcurrentDictionary<object, IClient> _platformToClient = new ConcurrentDictionary<object, IClient>();

        public IClient CreateFromPlatformObject(object platformObject)
        {
            return new PlatformClient((Client)platformObject);
        }

        public IClient FindByPlatformObject(object platformObject)
        {
            IClient client;
            if (_platformToClient.TryGetValue(platformObject, out client))
            {
                return client;
            }
            return null;
        }

        public void RegisterTuple(IClient client, object platformObject)
        {
            _platformToClient.TryAdd(platformObject, client);
        }
    }
}
