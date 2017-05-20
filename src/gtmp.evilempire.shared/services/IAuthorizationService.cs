using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IAuthorizationService
    {
        IServiceResult Authenticate(string username, string password);
    }
}
