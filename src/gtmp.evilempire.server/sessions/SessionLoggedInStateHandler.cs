﻿using System;
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
            }
            else
            {
                session.State = SessionState.LoggedIn;
            }
            return true;
        }
    }
}