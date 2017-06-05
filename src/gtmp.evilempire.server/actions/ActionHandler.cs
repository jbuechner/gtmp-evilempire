using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    abstract class ActionHandler
    {
        public ActionHandler(ServiceContainer services, IDictionary<string, object> arguments)
        {
        }

        public abstract void Handle(ISession session);
    }
}
