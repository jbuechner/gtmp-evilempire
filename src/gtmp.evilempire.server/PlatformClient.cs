using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared.Math;
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
        bool _isFrozen;

        public object PlatformObject
        {
            get
            {
                return _client;
            }
        }

        public bool IsNametagVisible
        {
            get
            {
                return _client.nametagVisible;
            }

            set
            {
                _client.nametagVisible = value;
            }
        }

        public string Name
        {
            get
            {
                return _client.name;
            }
        }

        public int Dimension
        {
            get
            {
                return _client.dimension;
            }
            set
            {
                _client.dimension = value;
            }
        }

        public bool CanMove
        {
            get
            {
                return !_isFrozen;
            }
            set
            {
                _client.freeze(_isFrozen = !value);
            }
        }

        public object Position
        {
            get
            {
                return _client.position;
            }
            set
            {
                _client.position = (Vector3)value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return !_client.IsNull;
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

        public void StopAnimation()
        {
            _client.stopAnimation();
        }

        public void TriggerClientEvent(string eventName, params object[] args)
        {
            _client.triggerEvent(eventName, args);
        }

        public void SetData(string key, object value)
        {
            _client.setData(key, value);
        }

        public object GetData(string key)
        {
            return _client.getData(key);
        }
    }
}
