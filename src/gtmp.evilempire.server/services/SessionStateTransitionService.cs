using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.services
{
    class SessionStateTransitionService : ISessionStateTransitionService
    {
        readonly Dictionary<SessionState, SessionStateHandlerBase> transitions = new Dictionary<SessionState, SessionStateHandlerBase>();

        ServiceContainer services;

        public SessionStateTransitionService(ServiceContainer services)
        {
            this.services = services;
        }

        public bool Transit(ISession session, SessionState newState)
        {
            SessionStateHandlerBase handler;
            if (transitions.TryGetValue(newState, out handler))
            {
                return handler?.Transit(session) ?? false;
            }

            return false;
        }

        public void RegisterTransition(SessionStateHandlerBase handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            transitions.Add(handler.HandledSessionState, handler);
        }
    }
}
