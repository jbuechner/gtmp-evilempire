using gtmp.evilempire.entities;

namespace gtmp.evilempire.services
{
    public interface IAuthorizationService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "login")]
        IServiceResult Authenticate(string login, string password);

        AuthUserGroup GetUserGroup(string login);
    }
}
