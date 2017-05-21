using GrandTheftMultiplayer.Shared.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapObject
    {
        public int Hash { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }

        public MapObject(int hash, Vector3 position, Vector3 rotation)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
        }
    }
}
