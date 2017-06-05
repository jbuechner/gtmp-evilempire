using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class MapDialogue : MapDialoguePage
    {
        public string Name { get; }

        public MapDialogue(string key, string markdown)
            : base(key, markdown)
        {
            Name = key;
        }
    }
}
