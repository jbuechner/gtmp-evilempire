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
        public IList<MapProp> Props { get; } = new List<MapProp>();
        public IList<MapVehicle> Vehicles { get; } = new List<MapVehicle>();

        public IList<MapPed> Peds { get; } = new List<MapPed>();

        Dictionary<MapPointType, Dictionary<int, MapPoint>> MapPointMap { get; } = new Dictionary<MapPointType, Dictionary<int, MapPoint>>();
        Dictionary<string, MapPoint> NamedMapPointMap { get; } = new Dictionary<string, MapPoint>();

        Dictionary<string, MapVehicle> TemplatedMapVehicles { get; } = new Dictionary<string, MapVehicle>();

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

        public MapVehicle FindVehicleByTemplateName(string templateName)
        {
            MapVehicle templateVehicle;
            if (TemplatedMapVehicles.TryGetValue(templateName, out templateVehicle))
            {
                return templateVehicle;
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

        public void AddMarker(MapMarker mapMarker)
        {
            Markers.Add(mapMarker);
        }

        public void AddProp(MapProp mapProp)
        {
            if (!mapProp.IsTemplate)
            {
                Props.Add(mapProp);
            }
        }

        public void AddPed(MapPed mapPed)
        {
            if (!mapPed.IsTemplate)
            {
                Peds.Add(mapPed);
            }
        }

        public void AddVehicle(MapVehicle mapVehicle)
        {
            if (mapVehicle.IsTemplate)
            {
                TemplatedMapVehicles.Add(mapVehicle.TemplateName, mapVehicle);
            }
            else
            {
                Vehicles.Add(mapVehicle);
            }
        }
    }
}
