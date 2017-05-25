using gtmp.evilempire.entities;
using gtmp.evilempire.entities.processors;
using gtmp.evilempire.services;
using System;

namespace gtmp.evilempire.server.services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class AuthorizationService : IAuthorizationService
    {
        IDbService DbService { get; }

        public AuthorizationService(IDbService dbService)
        {
            DbService = dbService;
        }

        public IServiceResult Authenticate(string login, string password)
        {
            var user = DbService.Select<User, string>(login);
            password = UserPasswordHashProcessor.Hash(password);
            if (user != null)
            {
                user.LastLogin = DateTime.Now;
                DbService.Update(user);
                if (string.CompareOrdinal(user.Password, password) == 0)
                {
                    return ServiceResult.AsSuccess();
                }
                else
                {
                    user.NumberOfInvalidLoginAttempts += 1;
                    DbService.Update(user);
                }
            }
            return ServiceResult.AsError("Authentication failed.");
        }

        public AuthUserGroup GetUserGroup(string login)
        {
            var user = DbService.Select<User, string>(login);
            if (user == null)
            {
                return AuthUserGroup.Guest;
            }
            return user.UserGroup;
        }
    }
}
