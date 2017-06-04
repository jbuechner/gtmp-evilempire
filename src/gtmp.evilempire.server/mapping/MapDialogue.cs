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

        public MapDialogue(string key, string markdown, MapDialogueAction action)
            : base(key, markdown, action)
        {
            Name = key;
        }
    }
}
