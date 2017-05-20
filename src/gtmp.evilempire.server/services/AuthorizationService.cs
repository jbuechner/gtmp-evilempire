using gtmp.evilempire.entities;
using gtmp.evilempire.entities.processors;
using gtmp.evilempire.services;

namespace gtmp.evilempire.server.services
{
    class AuthorizationService : IAuthorizationService
    {
        IDbService DbService { get; }
        UserPasswordHashProcessor PasswordHashProcesser { get; } = new UserPasswordHashProcessor();

        public AuthorizationService(IDbService dbService)
        {
            DbService = dbService;
        }

        public IServiceResult Authenticate(string username, string password)
        {
            var user = DbService.Select<User, string>(username);
            password = PasswordHashProcesser.Hash(password);
            if (user != null && string.CompareOrdinal(user.Password, password) == 0)
            {
                return ServiceResult.AsSuccess();
            }
            return ServiceResult.AsError(null);
        }
    }
}
