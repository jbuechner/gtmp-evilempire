using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class MapTimer
    {
        public TimeSpan Interval { get; set; }
        public IList<Item> Items { get; } = new List<Item>();
    }
}
