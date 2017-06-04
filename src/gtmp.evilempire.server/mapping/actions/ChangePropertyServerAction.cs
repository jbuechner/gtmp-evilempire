using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.mapping.actions
{
    class ChangePropertyServerAction : MapDialogueServerAction
    {
        public override string Name
        {
            get
            {
                return "Change";
            }
        }

        public ChangePropertyServerAction(ServiceContainer services)
            : base(services)
        {
        }

        public override bool PerformAction(ISession session, IDictionary<string, string> args)
        {
            return false;
        }
    }
}
