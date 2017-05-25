using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapRoute
    {
        public string Name { get; set; }
        public List<MapRoutePoint> Points { get; } = new List<MapRoutePoint>();
        public int Iterations { get; set; }
    }
}
