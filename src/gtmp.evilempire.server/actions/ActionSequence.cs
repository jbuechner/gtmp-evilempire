using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    class ActionSequence
    {
        public IList<ActionSequenceItem> Items { get; } = new List<ActionSequenceItem>();
    }
}
