using gtmp.evilempire.entities;
using gtmp.evilempire.entities.processors;
using gtmp.evilempire.services;

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
            var user = DbService.SelectEntity<User, string>(login);
            password = UserPasswordHashProcessor.Hash(password);
            if (user != null && string.CompareOrdinal(user.Password, password) == 0)
            {
                return ServiceResult.AsSuccess();
            }
            return ServiceResult.AsError("Authentication failed.");
        }
    }
}
