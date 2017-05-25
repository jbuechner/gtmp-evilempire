using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.mapping
{
    public class MapObject
    {
        public int Hash { get; }
        public Vector3f Position { get; }
        public Vector3f Rotation { get; }

        public MapObject(int hash, Vector3f position, Vector3f rotation)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
        }
    }
}
