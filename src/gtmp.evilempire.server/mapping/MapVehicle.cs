using GrandTheftMultiplayer.Shared.Math;

namespace gtmp.evilempire.server.mapping
{
    public class MapVehicle
    {
        public int Hash { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }

        public int Color1 { get; }
        public int Color2 { get; }

        public MapVehicle(int hash, Vector3 position, Vector3 rotation, int color1, int color2)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
            Color1 = color1;
            Color2 = color2;
        }
    }
}
