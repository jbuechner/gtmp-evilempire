using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    class ActionExecutionContext
    {
        public ISession Session { get; }
        public IDictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();

        public ActionExecutionContext(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            Session = session;
        }
    }
}
