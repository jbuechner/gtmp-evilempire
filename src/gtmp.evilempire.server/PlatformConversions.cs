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

        public static GrandTheftMultiplayer.Server.Constant.Color ToColor(this Color color)
        {
            return new GrandTheftMultiplayer.Server.Constant.Color(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static Color ToColor(this GrandTheftMultiplayer.Server.Constant.Color color)
        {
            return new Color((byte)color.red, (byte)color.green, (byte)color.blue, (byte)color.alpha);
        }
    }
}
