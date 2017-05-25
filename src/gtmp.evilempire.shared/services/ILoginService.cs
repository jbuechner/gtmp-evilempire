using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
    public interface ILoginService
    {
        User FindUserByLogin(string login);

        IClient FindLoggedInClientByLogin(string login);

        DateTime LastLoggedInClientsChangeTime { get; }
        void Purge();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "login")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        IServiceResult<User> Login(string login, IClient client);
        void Logout(IClient client);

        bool IsLoggedIn(IClient client);

        IList<IClient> GetLoggedInClients();
    }
}
