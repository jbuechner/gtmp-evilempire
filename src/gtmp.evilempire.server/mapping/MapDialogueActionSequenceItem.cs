using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapDialogueActionSequenceItem
    {
        public string Type { get; }
        public IDictionary<string, string> Args { get; }

        public MapDialogueActionSequenceItem(string type, IDictionary<string, string> args)
        {
            Type = type;
            Args = args;
        }
    }
}
