using GrandTheftMultiplayer.Shared.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapPed
    {
        public int Hash { get; }
        public Vector3 Position { get; }
        public float Rotation { get; }
        public bool IsInvincible { get; }

        public MapPed(int hash, Vector3 position, float rotation, bool isInvincible)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
            IsInvincible = isInvincible;
        }
    }
}
