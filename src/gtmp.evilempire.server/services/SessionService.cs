using gtmp.evilempire.services;
using System;
using gtmp.evilempire.sessions;
using System.Collections.Concurrent;
using gtmp.evilempire.server.sessions;

namespace gtmp.evilempire.server.services
{
    class SessionService : ISessionService
    {
        ConcurrentDictionary<Session, byte> sessions = new ConcurrentDictionary<Session, byte>();
        ConcurrentDictionary<IClient, Session> clientToSessionMap = new ConcurrentDictionary<IClient, Session>();
        ConcurrentDictionary<string, Session> loginToSessionMap = new ConcurrentDictionary<string, Session>();

        ISessionStateTransitionService sessionStateTransitionService;

        public SessionService(ISessionStateTransitionService sessionStateTransitionService)
        {
            this.sessionStateTransitionService = sessionStateTransitionService;
        }

        public ISession CreateSession(IClient client)
        {
            var session = new Session(client);
            sessions.TryAdd(session, 1);
            clientToSessionMap.TryAdd(client, session);
            return session;
        }

        public ISession GetSession(IClient client)
        {
            Session session;
            if (clientToSessionMap.TryGetValue(client, out session))
            {
                return session;
            }
            return null;
        }

        public ISession GetSessionByLogin(string login)
        {
            Session session;
            if (loginToSessionMap.TryGetValue(login, out session))
            {
                return session;
            }
            return null;
        }

        public void AssociateSessionWithLogin(ISession session, string login)
        {
            var sessionObject = session as Session;
            if (sessionObject == null)
            {
                throw new NotImplementedException();
            }
            loginToSessionMap.AddOrUpdate(login, sessionObject, (k, v) => sessionObject);
        }
    }
}
