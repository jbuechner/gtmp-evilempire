using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping.actions
{
    public abstract class MapDialogueServerAction
    {
        public abstract string Name { get; }

        public MapDialogueServerAction(ServiceContainer services)
        {
        }

        public abstract bool PerformAction(ISession session, IDictionary<string, string> args);
    }
}
