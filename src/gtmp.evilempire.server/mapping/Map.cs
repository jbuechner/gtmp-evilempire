using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    public class Map
    {
        public IList<MapPoint> Points { get; } = new List<MapPoint>();
        public IList<MapMarker> Markers { get; } = new List<MapMarker>();
        public IList<MapObject> Objects { get; } = new List<MapObject>();
        public IList<MapVehicle> Vehicles { get; } = new List<MapVehicle>();

        public IList<MapPed> Peds { get; } = new List<MapPed>();

        Dictionary<MapPointType, Dictionary<int, MapPoint>> MapPointMap { get; } = new Dictionary<MapPointType, Dictionary<int, MapPoint>>();

        public MapPoint GetPoint(MapPointType mapPointType, int id)
        {
            Dictionary<int, MapPoint> map;
            if (MapPointMap.TryGetValue(mapPointType, out map))
            {
                MapPoint mapPoint;
                if (map.TryGetValue(id, out mapPoint))
                {
                    return mapPoint;
                }
            }
            return null;
        }

        public void AddPoint(MapPoint mapPoint)
        {
            Points.Add(mapPoint);
            Dictionary<int, MapPoint> map;
            if (!MapPointMap.TryGetValue(mapPoint.Type, out map))
            {
                map = MapPointMap[mapPoint.Type] = new Dictionary<int, MapPoint>();
            }
            map[mapPoint.Id] = mapPoint;
        }

        public void AddMarker(MapMarker mapMarker)
        {
            Markers.Add(mapMarker);
        }

        public void AddObject(MapObject mapObject)
        {
            Objects.Add(mapObject);
        }

        public void AddPed(MapPed mapPed)
        {
            Peds.Add(mapPed);
        }

        public void AddVehicle(MapVehicle mapVehicle)
        {
            Vehicles.Add(mapVehicle);
        }
    }
}
