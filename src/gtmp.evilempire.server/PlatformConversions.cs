using GrandTheftMultiplayer.Shared.Math;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server
{
    public static class PlatformConversions
    {
        public static Vector3f ToVector3f(this Vector3 vector)
        {
            return new Vector3f(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToVector3(this Vector3f vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
