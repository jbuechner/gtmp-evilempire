﻿using GrandTheftMultiplayer.Shared.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class MapPoint
    {
        public int Id { get; }
        public MapPointType Type { get; }
        public Vector3 Position { get; }

        public MapPoint(MapPointType type, int id, Vector3 position)
        {
            Type = type;
            Id = id;
            Position = position;
        }
    }
}