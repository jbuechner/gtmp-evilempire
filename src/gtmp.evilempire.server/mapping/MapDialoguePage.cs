using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapDialoguePage
    {
        public string Key { get; }
        public string Markdown { get; }
        public MapDialogueAction Action { get; }
        public IList<MapDialoguePage> Pages { get; } = new List<MapDialoguePage>();

        public MapDialoguePage(string key, string markdown, MapDialogueAction action)
        {
            Key = key;
            Markdown = markdown;
            Action = action;
        }

    }
}
