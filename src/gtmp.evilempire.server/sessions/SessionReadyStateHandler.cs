using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.sessions
{
    class SessionReadyStateHandler : SessionStateHandlerBase
    {
        public override SessionState HandledSessionState
        {
            get
            {
                return SessionState.Ready;
            }
        }

        public SessionReadyStateHandler(ServiceContainer services)
            : base(services)
        {
        }

        public override bool Transit(ISession session)
        {
            var client = session.Client;
            client.TriggerClientEvent(ClientEvents.DisplayLoginScreen);
            session.State = SessionState.Ready;
            return true;
        }
    }
}
