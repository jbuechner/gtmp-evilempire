using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.services
{
    class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService()
        {
        }

        public IServiceResult Authenticate(string username, string password)
        {
            //::fe4873ee8e66fc12ff940d6a21e1ff73962874c56bbed7f41c95247a086ebb362797e60cd413e6f5ceada814d30a96349038686e365a09ecc030d9ad188286c3 abc
            throw new NotImplementedException($"username={username}, password={password}");
        }
    }
}
