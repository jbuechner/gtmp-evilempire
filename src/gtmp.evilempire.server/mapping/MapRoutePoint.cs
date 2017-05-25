using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapRoutePoint
    {
        public string Name { get; set; }
        public MapPoint MapPoint { get; set; }
        public bool IsStart { get; set; }
    }
}
