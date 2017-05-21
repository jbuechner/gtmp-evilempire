using GrandTheftMultiplayer.Server.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class ServerMapLoader
    {
        public static void Load(Map map, API api)
        {
            foreach(var marker in map.Markers)
            {
                api.createMarker((int)marker.Type, marker.Position, marker.Direction, marker.Rotation, marker.Scale, marker.Alpha, marker.Red, marker.Green, marker.Blue);
            }
        }
    }
}
