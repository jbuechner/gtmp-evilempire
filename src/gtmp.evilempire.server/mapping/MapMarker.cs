using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.mapping
{
    public class MapMarker
    {
        public MarkerType MarkerType { get; }
        public Vector3f Position { get; }
        public Vector3f Rotation { get; }
        public Vector3f Direction { get; }
        public Vector3f Scale { get; }
        public byte Alpha { get; }
        public byte Red { get; }
        public byte Blue { get; }
        public byte Green { get; }

        public MapMarker(MarkerType markerType, Vector3f position, Vector3f direction, Vector3f rotation, Vector3f scale, byte alpha, byte red, byte green, byte blue)
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
