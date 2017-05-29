using gtmp.evilempire.entities;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.sessions
{
    class Session : ISession
    {
        IClient client;

        public IClient Client
        {
            get
            {
                return client;
            }
        }

        public SessionState State { get; set; }

        public User User { get; set; }
        public Character Character { get; set; }
        public CharacterCustomization CharacterCustomization { get; set; }
        public int PrivateDimension { get; set; }

        public bool UpdateDatabasePosition { get; set; }

        public Session(IClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            this.client = client;
        }
    }
}
