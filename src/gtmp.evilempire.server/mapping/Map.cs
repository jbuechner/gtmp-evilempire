using gtmp.evilempire.entities;
using gtmp.evilempire.server.mapping.actions;
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
        public IList<Vehicle> Vehicles { get; } = new List<Vehicle>();

        public IList<MapPed> Peds { get; } = new List<MapPed>();

        public IList<MapBlip> Blips { get; } = new List<MapBlip>();

        Dictionary<MapPointType, Dictionary<int, MapPoint>> MapPointMap { get; } = new Dictionary<MapPointType, Dictionary<int, MapPoint>>();
        Dictionary<string, MapPoint> NamedMapPointMap { get; } = new Dictionary<string, MapPoint>();

        Dictionary<string, Vehicle> TemplatedMapVehicles { get; } = new Dictionary<string, Vehicle>();

        Dictionary<string, MapDialogue> Dialogues { get; } = new Dictionary<string, MapDialogue>();

        Dictionary<int, MapPed> RuntimeHandleToPedMap { get; } = new Dictionary<int, MapPed>();

        Dictionary<string, MapDialogueServerAction> MapDialogueServerActions = new Dictionary<string, MapDialogueServerAction>();

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

        public MapDialogue GetDialogue(string key)
        {
            MapDialogue dialogue;
            if (Dialogues.TryGetValue(key, out dialogue))
            {
                return dialogue;
            }
            return null;
        }

        public Vehicle FindVehicleByTemplateName(string templateName)
        {
            Vehicle templateVehicle;
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

        public void AddVehicle(Vehicle mapVehicle)
        {
            if (!string.IsNullOrEmpty(mapVehicle.TemplateName))
            {
                TemplatedMapVehicles.Add(mapVehicle.TemplateName, mapVehicle);
            }
            else
            {
                Vehicles.Add(mapVehicle);
            }
        }

        public void AddBlip(MapBlip mapBlip)
        {
            Blips.Add(mapBlip);
        }

        public void AddDialogue(MapDialogue mapDialogue)
        {
            if (mapDialogue == null)
            {
                throw new ArgumentNullException(nameof(mapDialogue));
            }
            Dialogues[mapDialogue.Key] = mapDialogue;
        }

        public void MakeAssociation(int runtimeHandle, MapPed ped)
        {
            RuntimeHandleToPedMap[runtimeHandle] = ped;
        }

        public MapPed GetPedByRuntimeHandle(int runtimeHandle)
        {
            MapPed ped;
            if (RuntimeHandleToPedMap.TryGetValue(runtimeHandle, out ped))
            {
                return ped;
            }
            return null;
        }

        public void AddDialogueServerAction(MapDialogueServerAction serverAction)
        {
            MapDialogueServerActions[serverAction.Name] = serverAction;
        }

        public MapDialogueServerAction FindDialogueServerActionByName(string name)
        {
            MapDialogueServerAction serverAction;
            if (MapDialogueServerActions.TryGetValue(name, out serverAction))
            {
                return serverAction;
            }
            return null;
        }
    }
}
