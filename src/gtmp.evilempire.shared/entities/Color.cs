using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public struct Color
    {
        public static readonly Color Black = new Color(0, 0, 0);

        byte r;
        byte g;
        byte b;
        byte a;

        public byte Red { get { return r; } set { r = value; } }
        public byte Green { get { return g; } set { g = value; } }
        public byte Blue { get { return b; } set { b = value; } }
        public byte Alpha { get { return a; } set { a = value; } }

        public Color(byte red, byte green, byte blue)
        {
            r = red;
            g = green;
            b = blue;
            a = 0;
        }

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
        }
    }
}
