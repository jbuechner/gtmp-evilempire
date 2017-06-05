using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class MapMetadata
    {
        public IList<Item> StartingInventoryItems { get; } = new List<Item>();
    }
}
