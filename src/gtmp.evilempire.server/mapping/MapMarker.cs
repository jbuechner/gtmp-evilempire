using GrandTheftMultiplayer.Shared.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapMarker
    {
        public MarkerType MarkerType { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public Vector3 Direction { get; }
        public Vector3 Scale { get; }
        public byte Alpha { get; }
        public byte Red { get; }
        public byte Blue { get; }
        public byte Green { get; }

        public MapMarker(MarkerType markerType, Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, byte alpha, byte red, byte green, byte blue)
        {
            MarkerType = markerType;
            Position = position;
            Direction = direction;
            Rotation = rotation;
            Scale = scale;
            Alpha = alpha;
            Red = red;
            Blue = blue;
            Green = green;
        }
    }
}
