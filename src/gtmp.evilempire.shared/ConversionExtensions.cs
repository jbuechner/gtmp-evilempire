using gtmp.evilempire.entities;
using System.Globalization;

namespace gtmp.evilempire
{
    public static class ConversionExtensions
    {
        public static string AsString(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is string)
            {
                return (string)value;
            }
            return value.ToString();
        }

        public static int? AsInt(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is int)
            {
                return (int)value;
            }
            if (value is string)
            {
                int v;
                if (int.TryParse((string)value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                {
                    return v;
                }
            }
            return null;
        }

        public static byte? AsByte(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is byte)
            {
                return (byte)value;
            }
            if (value is string)
            {
                byte v;
                if (byte.TryParse((string)value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                {
                    return v;
                }
            }
            return null;
        }

        public static bool? AsBool(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is bool)
            {
                return (bool)value;
            }
            if (value is string)
            {
                return bool.Parse((string)value);
            }
            return null;
        }

        public static float? AsFloat(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is float)
            {
                return (float)value;
            }
            if (value is string)
            {
                float v;
                if (float.TryParse((string)value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                {
                    return v;
                }
            }
            return null;
        }

        public static Vector3f? AsVector3f(this string value)
        {
            if (value == null)
            {
                return null;
            }
            var parts = value.Split(',');
            if (parts != null && parts.Length == 3)
            {
                var x = parts[0].AsFloat() ?? 0;
                var y = parts[1].AsFloat() ?? 0;
                var z = parts[2].AsFloat() ?? 0;
                return new Vector3f(x, y, z);
            }
            return null;
        }
    }
}
