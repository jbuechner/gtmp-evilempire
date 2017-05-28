using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface ISessionStateTransitionService
    {
        bool Transit(ISession session, SessionState newState);
        void RegisterTransition(SessionStateHandlerBase sessionStateHandler);
    }
}
