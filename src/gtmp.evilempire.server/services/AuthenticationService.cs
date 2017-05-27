using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.entities;
using gtmp.evilempire.entities.processors;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.services
{
    class AuthenticationService : IAuthenticationService
    {
        IDbService db;
        ISessionService sessions;

        public AuthenticationService(IDbService db, ISessionService sessions)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }
            if (sessions == null)
            {
                throw new ArgumentNullException(nameof(sessions));
            }

            this.db = db;
            this.sessions = sessions;
        }

        public User Authenticate(ISession session, string login, string password)
        {
            var user = FindUserByLogin(login);
            if (user == null)
            {
                return null;
            }

            user.LastLogin = DateTime.Now;

            var otherSession = sessions.GetSessionByLogin(login);
            if (otherSession != null && !session.Client.Equals(otherSession?.Client))
            {
                user.NumberOfInvalidLoginAttempts += 1;
                db.Update(user);
                return null;
            }

            password = UserPasswordHashProcessor.Hash(password);
            if (string.CompareOrdinal(user.Password, password) != 0)
            {
                user.NumberOfInvalidLoginAttempts += 1;
                db.Update(user);
                return null;
            }

            if (user.FirstLogin == null)
            {
                user.FirstLogin = DateTime.Now;
            }

            user.NumberOfInvalidLoginAttempts = 0;
            user.LastSuccessfulLogin = DateTime.Now;
            db.Update(user);

            session.User = user;
            sessions.AssociateSessionWithLogin(session, login);

            return user;
        }

        public User FindUserByLogin(string login)
        {
            var user = db.Select<User, string>(login);
            return user;
        }
    }
}
