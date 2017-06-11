using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.httprpc
{
    abstract class HttpListenerRoute
    {

        public abstract bool Matches(HttpListenerContext context);

        public abstract void Handle(HttpListenerContext context);
    }
}
