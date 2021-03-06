﻿using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.mapping
{
    public class MapPoint
    {
        public int Id { get; }
        public string Name { get; }
        public MapPointType PointType { get; }
        public Vector3f Position { get; }

        public Vector3f? Rotation { get; set; }

        public MapPoint(MapPointType type, int id, string name, Vector3f position, Vector3f? rotation)
        {
            PointType = type;
            Id = id;
            Name = name;
            Position = position;
            Rotation = rotation;
        }
    }
}
