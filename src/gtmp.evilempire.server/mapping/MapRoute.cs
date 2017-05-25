using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapRoute
    {
        public string Name { get; }
        public IList<MapRoutePoint> Points { get; } = new List<MapRoutePoint>();
        public int Iterations { get; }

        public IList<MapTemplateReference> Objects { get; } = new List<MapTemplateReference>();

        public MapRoute(string name, int iterations)
        {
            Name = name;
            Iterations = iterations;
        }
    }
}
