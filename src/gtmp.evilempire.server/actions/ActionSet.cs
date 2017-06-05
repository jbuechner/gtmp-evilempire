using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    class ActionSet
    {
        public IList<ActionSetItem> Actions { get; } = new List<ActionSetItem>();
    }
}
