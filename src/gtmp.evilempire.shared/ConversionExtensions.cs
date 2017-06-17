using gtmp.evilempire.entities;
using System;
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
            return AsInt(value, NumberStyles.Integer);
        }

        public static int? AsInt(this object value, NumberStyles numberStyles)
        {
            if (value == null)
            {
                return null;
            }
            if (value is int)
            {
                return (int)value;
            }
            string raw = (value as string) ?? value.ToString();
            int v;
            if (raw != null && raw.IndexOf("0x", StringComparison.OrdinalIgnoreCase) == 0)
            {
                numberStyles = NumberStyles.HexNumber;
                raw = raw.Substring(2);
            }

            if (int.TryParse(raw, numberStyles, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            return null;
        }

        public static long? AsLong(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is long)
            {
                return (long)value;
            }
            string raw = (value as string) ?? value.ToString();
            long v;
            if (long.TryParse(raw, out v))
            {
                return v;
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
            string raw = (value as string) ?? value.ToString();
            byte v;
            if (byte.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
            {
                return v;
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
                try
                {
                    return bool.Parse((string)value);
                }
                catch(FormatException)
                {
                    return null;
                }
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
            string raw = (value as string) ?? value.ToString();
            float v;
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            return null;
        }

        public static double? AsDouble(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is double)
            {
                return (double)value;
            }
            string raw = (value as string) ?? value.ToString();
            double v;
            if (double.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out v))
            {
                return v;
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

        public static TimeSpan? AsTimeSpan(this object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is TimeSpan)
            {
                return (TimeSpan)value;
            }
            string raw = (value as string) ?? value.ToString();
            TimeSpan timeSpan;
            if (TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out timeSpan))
            {
                return timeSpan;
            }
            return null;
        }
    }
}
