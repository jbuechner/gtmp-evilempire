using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.sessions
{
    public abstract class SessionStateHandlerBase
    {
        ServiceContainer services;

        public abstract SessionState HandledSessionState { get; }

        public SessionStateHandlerBase(ServiceContainer services)
        {
            this.services = services;
        }

        public abstract bool Transit(ISession session);
    }
}
