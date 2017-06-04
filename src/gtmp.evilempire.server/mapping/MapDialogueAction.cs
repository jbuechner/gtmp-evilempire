using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapDialogueAction
    {
        public bool IsClientSide { get; }
        public string Name { get; }

        public IList<MapDialogueActionSequenceItem> Sequence { get; } = new List<MapDialogueActionSequenceItem>();

        public MapDialogueAction(bool isClientSide, string name)
        {
            IsClientSide = isClientSide;
            Name = name;
        }
    }
}
