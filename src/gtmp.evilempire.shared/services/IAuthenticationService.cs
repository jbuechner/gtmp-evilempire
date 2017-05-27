using gtmp.evilempire.entities;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IAuthenticationService
    {
        User Authenticate(ISession session, string login, string password);
        User FindUserByLogin(string login);
    }
}
