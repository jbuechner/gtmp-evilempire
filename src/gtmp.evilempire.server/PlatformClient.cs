using GrandTheftMultiplayer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server
{
    class PlatformClient : IClient
    {
        Client _client;

        public object PlatformObject
        {
            get
            {
                return _client;
            }
        }

        public PlatformClient(Client client)
        {
            _client = client;
        }

        public bool Equals(IClient other)
        {
            var otherClient = other as PlatformClient;
            if (otherClient != null)
            {
                return Equals(otherClient);
            }
            return false;
        }

        public bool Equals(PlatformClient client)
        {
            var a = _client;
            var b = client._client;
            return string.Equals(a.socialClubName, b.socialClubName, StringComparison.Ordinal)
                && string.Equals(a.address, b.address, StringComparison.Ordinal)
                && !a.IsNull && !b.IsNull
                && (a.exists && !b.exists || (!a.handle.IsNull && !b.handle.IsNull) && a.handle.Value == b.handle.Value);
        }
    }
}
