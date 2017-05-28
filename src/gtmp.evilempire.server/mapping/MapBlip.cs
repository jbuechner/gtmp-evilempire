using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapBlip
    {
        public Vector3f Position { get; set; }
        public int Sprite { get; set; }
        public int Color { get; set; }

        public MapBlip(Vector3f position, int sprite, int color)
        {
            Position = position;
            Sprite = sprite;
            Color = color;
        }
    }
}
