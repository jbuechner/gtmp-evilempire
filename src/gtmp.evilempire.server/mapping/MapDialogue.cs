using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapDialogue : MapDialoguePage
    {
        public string Name { get; }

        public MapDialogue(string key, string markdown, string action, bool isClientSideAction)
            : base(key, markdown, action, isClientSideAction)
        {
            Name = key;
        }
    }
}
