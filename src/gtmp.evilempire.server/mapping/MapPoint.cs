using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.mapping
{
    public class MapPoint
    {
        public int Id { get; }
        public MapPointType PointType { get; }
        public Vector3f Position { get; }

        public MapPoint(MapPointType type, int id, Vector3f position)
        {
            PointType = type;
            Id = id;
            Position = position;
        }
    }
}
