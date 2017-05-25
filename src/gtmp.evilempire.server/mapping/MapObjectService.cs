using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gtmp.evilempire.server.mapping
{
    public class MapObjectService
    {
        List<MapObjectScheduler> ActiveSchedulers { get; } = new List<MapObjectScheduler>();

        Map Map { get; }

        public MapObjectService(Map map)
        {
            Map = map;
        }

        public void StartAllRoutes()
        {
            if (Map.Routes != null)
            {
                foreach(var route in Map.Routes)
                {
                    if (route == null)
                    {
                        continue;
                    }
                    StartRoute(route);
                }
            }
        }

        public void StartRoute(MapRoute route)
        {
            var scheduler = new MapObjectScheduler();
            var startPoint = GetStartPoint(route);
            foreach(var obj in route.Objects)
            {
                var templateInstance = CreateMapObjectByTemplate(obj);
                PositionObject(templateInstance, startPoint.Position, startPoint.Rotation);
                scheduler.AddObject(templateInstance);
            }
            scheduler.Iterations = route.Iterations;
            foreach(var routePoint in route.Points)
            {
                if (!routePoint.IsStart)
                {
                    scheduler.AddMapPoint(routePoint);
                }
            }
            scheduler.Start();
            ActiveSchedulers.Add(scheduler);
        }

        public object CreateMapObjectByTemplate(MapTemplateReference mapTemplateReference)
        {

            switch(mapTemplateReference.MapObjectType)
            {
                case MapObjectType.Vehicle:
                    var template = Map.FindVehicleByTemplateName(mapTemplateReference.TemplateName);
                    return ServerMapLoader.Load(template, API.shared);
                default:
                    throw new NotImplementedException($"Can not create map object type for template reference of type \"{mapTemplateReference.MapObjectType}\".");
            }
        }

        MapPoint GetStartPoint(MapRoute route)
        {
            var startPoint = route.Points.FirstOrDefault(p => p.IsStart);
            if (startPoint == null)
            {
                startPoint = route.Points.FirstOrDefault();
            }
            return startPoint?.MapPoint;
        }

        void PositionObject(object mapObject, Vector3f position, Vector3f? rotation)
        {
            if (mapObject is Vehicle)
            {
                var vehicle = (Vehicle)mapObject;
                vehicle.position = position.ToVector3();
                if (rotation.HasValue)
                {
                    vehicle.rotation = rotation.Value.ToVector3();
                }
                return;
            }
            throw new NotImplementedException($"Can not position object of type \"{mapObject.GetType().Name}\".");
        }
    }
}
