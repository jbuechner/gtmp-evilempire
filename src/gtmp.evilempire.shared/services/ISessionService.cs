using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface ISessionService
    {
        ISession CreateSession(IClient client);
        ISession GetSession(IClient client);
        ISession GetSessionByLogin(string login);
        void AssociateSessionWithLogin(ISession session, string login);
        void RemoveSession(ISession session);
        void RemoveStaleSessions();
        void StoreSessionState();
    }
}
