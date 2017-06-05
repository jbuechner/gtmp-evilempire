using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    class ActionSequenceItem
    {
        public string ActionType { get; set; }
        public IDictionary<string, object> Args { get; } = new Dictionary<string, object>();
    }
}
