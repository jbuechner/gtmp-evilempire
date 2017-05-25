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

        public IList<MapRoute> Routes { get; } = new List<MapRoute>();

        Dictionary<MapPointType, Dictionary<int, MapPoint>> MapPointMap { get; } = new Dictionary<MapPointType, Dictionary<int, MapPoint>>();
        Dictionary<string, MapPoint> NamedMapPointMap { get; } = new Dictionary<string, MapPoint>();

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

        public MapPoint GetPointByName(string name)
        {
            MapPoint mapPoint;
            if (NamedMapPointMap.TryGetValue(name, out mapPoint))
            {
                return mapPoint;
            }
            return null;
        }

        public void AddPoint(MapPoint mapPoint)
        {
            if (mapPoint == null)
            {
                return;
            }

            Points.Add(mapPoint);

            if (mapPoint.PointType == MapPointType.Named)
            {
                NamedMapPointMap[mapPoint.Name] = mapPoint;
            }
            else
            {
                Dictionary<int, MapPoint> map;
                if (!MapPointMap.TryGetValue(mapPoint.PointType, out map))
                {
                    map = MapPointMap[mapPoint.PointType] = new Dictionary<int, MapPoint>();
                }
                map[mapPoint.Id] = mapPoint;
            }
        }

        public void AddRoute(MapRoute mapRoute)
        {
            Routes.Add(mapRoute);
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
