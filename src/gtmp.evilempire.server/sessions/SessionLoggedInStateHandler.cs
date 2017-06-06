using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;

namespace gtmp.evilempire.server.sessions
{
    class SessionLoggedInStateHandler : SessionStateHandlerBase
    {
        ISessionStateTransitionService sessionStateTransition;

        public override SessionState HandledSessionState
        {
            get
            {
                return SessionState.LoggedIn;
            }
        }

        public SessionLoggedInStateHandler(ServiceContainer services)
            : base(services)
        {
            sessionStateTransition = services.Get<ISessionStateTransitionService>();
        }

        public override bool Transit(ISession session)
        {
            if (!session.Character.HasBeenThroughInitialCustomization)
            {
                sessionStateTransition.Transit(session, SessionState.CharacterCustomization);
                session.State = SessionState.CharacterCustomization;
            }
            else
            {
                sessionStateTransition.Transit(session, SessionState.Freeroam);
                session.State = SessionState.Freeroam;
            }
            return true;
        }
    }
}
