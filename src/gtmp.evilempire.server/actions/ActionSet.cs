using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    class ActionSet
    {
        public ActionConditionalOperation Condition { get; set; }

        public IList<ActionSetItem> ThenActions { get; } = new List<ActionSetItem>();
        public IList<ActionSetItem> ElseActions { get; } = new List<ActionSetItem>();
    }
}
