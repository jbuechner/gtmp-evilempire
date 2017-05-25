using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapRoutePoint
    {
        public string Name { get; }
        public MapPoint MapPoint { get; set; }
        public bool IsStart { get; }
        public int Duration { get; }

        public MapRoutePoint(string name, bool isStart, int duration)
        {
            Name = name;
            IsStart = isStart;
            Duration = duration;
        }
    }
}
