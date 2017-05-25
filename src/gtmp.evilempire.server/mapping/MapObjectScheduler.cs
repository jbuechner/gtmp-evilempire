using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class MapObjectScheduler
    {
        class MapObjectScheduleState
        {
            public int iteration;
            public int nextMapPointIndex;
        }

        public int Iterations { get; set; }
        public bool IsRunning { get; set; }

        List<MapRoutePoint> RoutePoints { get; } = new List<MapRoutePoint>();

        Dictionary<object, MapObjectScheduleState> ObjectState { get; } = new Dictionary<object, MapObjectScheduleState>();

        public void AddObject(object mapObject)
        {
            ObjectState.Add(mapObject, new MapObjectScheduleState { nextMapPointIndex = 0 });
        }

        public void AddMapPoint(MapRoutePoint mapRoutePoint)
        {
            RoutePoints.Add(mapRoutePoint);
        }

        public void Start()
        {
            foreach(var obj in ObjectState)
            {
                MoveToNext(obj.Key, obj.Value);
            }
        }

        void MoveToNext(object obj, MapObjectScheduleState state)
        {
            var nextMapPoint = RoutePoints[state.nextMapPointIndex];
            if (++state.nextMapPointIndex >= RoutePoints.Count)
            {
                state.iteration += 1;
                state.nextMapPointIndex = 0;

                if (state.iteration > Iterations)
                {
                    return;
                }
            }
            var point = nextMapPoint.MapPoint;
            MoveObject(nextMapPoint.Duration, obj, point.Position, point.Rotation);
            Task.Delay(nextMapPoint.Duration).ContinueWith(t => MoveToNext(obj, state));
        }

        void MoveObject(int duration, object mapObject, Vector3f position, Vector3f? rotation)
        {
            if (mapObject is Vehicle)
            {
                var vehicle = (Vehicle)mapObject;
                vehicle.engineStatus = true;
                vehicle.movePosition(position.ToVector3(), duration);
                if (rotation.HasValue)
                {
                    vehicle.moveRotation(rotation.Value.ToVector3(), duration);
                }
                return;
            }
            throw new NotImplementedException($"Can not move object of type \"{mapObject.GetType().Name}\".");
        }
    }
}
