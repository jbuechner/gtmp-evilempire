using gtmp.evilempire.server.actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class MapDialoguePage
    {
        public string Key { get; }
        public string Markdown { get; }
        public IList<MapDialoguePage> Pages { get; } = new List<MapDialoguePage>();
        public IList<ActionSet> Actions { get; set; }

        public MapDialoguePage(string key, string markdown)
        {
            Key = key;
            Markdown = markdown;
        }
    }
}
