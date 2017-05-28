using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gtmp.evilempire.server.mapping
{
    public class MapLoader
    {
        IList<Action<Map, XDocument>> Handlers { get; }

        public MapLoader()
        {
            Handlers = new List<Action<Map, XDocument>> { LoadMarkers, LoadMapPoints, LoadProps, LoadPeds, LoadVehicles, LoadBlips };
        }

        public static Map LoadFrom(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(directory);
            }

            var mapLoader = new MapLoader();
            var map = new Map();
            var files = Directory.GetFiles(directory, "*.xml");
            foreach (var file in files)
            {
                mapLoader.Load(file, map);
            }
            return map;
        }

        public void Load(string file, Map map)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(file);
            }
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var xdoc = XDocument.Load(stream);
                foreach (var handler in Handlers)
                {
                    handler(map, xdoc);
                }
            }
        }

        void LoadProps(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Prop"))
            {
                var templateName = mapObject.Element("TemplateName")?.Value;
                var hash = mapObject.Element("Hash")?.Value.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.One;

                var obj = new MapProp(templateName, hash, position, rotation);
                map.AddProp(obj);
            }
        }

        void LoadPeds(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Ped"))
            {
                var templateName = mapObject.Element("TemplateName")?.Value;
                var hash = mapObject.Element("Hash")?.Value?.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.One;
                var isInvincible = mapObject.Element("Invicible")?.Value?.AsBool() ?? false;

                var ped = new MapPed(templateName, hash, position, rotation.Z, isInvincible);
                map.AddPed(ped);
            }
        }

        void LoadVehicles(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Vehicle"))
            {
                var templateName = mapObject.Element("TemplateName")?.Value;
                var hash = mapObject.Element("Hash")?.Value?.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.One;
                var color1 = mapObject.Element("PrimaryColor")?.Value?.AsInt() ?? 0;
                var color2 = mapObject.Element("SecondaryColor")?.Value?.AsInt() ?? 0;

                var vehicle = new MapVehicle(templateName, hash, position, rotation, color1, color2);

                // optional
                vehicle.IsInvincible = mapObject.Element("IsInvincible")?.Value?.AsBool() ?? false;
                vehicle.IsLocked = mapObject.Element("IsLocked")?.Value?.AsBool() ?? false;
                vehicle.IsCollisionless = mapObject.Element("IsCollisionless")?.Value?.AsBool() ?? false;
                vehicle.IsEngineRunning = mapObject.Element("IsEngineRunning")?.Value?.AsBool() ?? false;
                vehicle.HasBulletproofTyres = mapObject.Element("HasBulletproofTyres")?.Value?.AsBool() ?? false;
                vehicle.IsPositionFrozen = mapObject.Element("IsPositionFrozen")?.Value?.AsBool() ?? false;
                vehicle.NumberPlate = mapObject.Element("NumberPlate")?.Value;
                vehicle.NumberPlateStyle = mapObject.Element("NumberPlateStyle")?.Value.AsInt();
                vehicle.IsSpecialLightEnabled = mapObject.Element("IsSpecialLightEnabled")?.Value?.AsBool() ?? false;
                vehicle.TrimColor = mapObject.Element("TrimColor")?.Value.AsInt();

                vehicle.BrokenWindows = mapObject.Element("BrokenWindows")?.Attribute("Value")?.Value?.Split(',').Select(s => s.AsInt()).Where(p => p.HasValue).Select(s => s.Value).ToList();
                vehicle.OpenedDoors = mapObject.Element("OpenedDoors")?.Attribute("Value")?.Value?.Split(',').Select(s => s.AsInt()).Where(p => p.HasValue).Select(s => s.Value).ToList();
                vehicle.BrokenDoors = mapObject.Element("BrokenDoors")?.Attribute("Value")?.Value?.Split(',').Select(s => s.AsInt()).Where(p => p.HasValue).Select(s => s.Value).ToList();
                vehicle.PoppedTyres = mapObject.Element("PoppedTyres")?.Attribute("Value")?.Value?.Split(',').Select(s => s.AsInt()).Where(p => p.HasValue).Select(s => s.Value).ToList();

                var neonElements = mapObject.Elements("Neon");
                if(neonElements != null)
                {
                    List<MapVehicle.Neon> neons = new List<MapVehicle.Neon>();
                    foreach(var neonElement in neonElements)
                    {
                        var neonIndex = neonElement.Attribute("Slot")?.Value?.AsInt() ?? 0;
                        var isTurnedOn = neonElement.Attribute("On")?.Value?.AsBool() ?? false;
                        var neon = new MapVehicle.Neon { Index = neonIndex, IsTurnedOn = isTurnedOn };
                        neons.Add(neon);
                    }
                    vehicle.Neons = neons;
                }

                vehicle.EnginePowerMultiplier = mapObject.Element("EnginePowerMultiplier")?.Value.AsFloat();
                vehicle.EngineTorqueMultiplier = mapObject.Element("EngineTorqueMultiplier")?.Value.AsFloat();
                vehicle.CustomPrimaryColor = mapObject.Element("CustomPrimaryColor")?.ToColor();
                vehicle.CustomSecondaryColor = mapObject.Element("CustomSecondaryColor")?.ToColor();
                vehicle.ModColor1 = mapObject.Element("ModColor1")?.ToColor();
                vehicle.ModColor2 = mapObject.Element("ModColor2")?.ToColor();
                vehicle.NeonColor = mapObject.Element("NeonColor")?.ToColor();
                vehicle.TyreSmokeColor = mapObject.Element("TyreSmokeColor")?.ToColor();

                vehicle.WheelColor = mapObject.Element("WheelColor")?.Value.AsInt();
                vehicle.WheelType = mapObject.Element("WheelType")?.Value.AsInt();
                vehicle.WindowTint = mapObject.Element("WindowTint")?.Value.AsInt();
                vehicle.DashboardColor = mapObject.Element("DashboardColor")?.Value.AsInt();
                vehicle.Health = mapObject.Element("Health")?.Value.AsFloat();
                vehicle.Livery = mapObject.Element("Livery")?.Value.AsInt();
                vehicle.PearlescentColor = mapObject.Element("PearlescentColor")?.Value.AsInt();

                map.AddVehicle(vehicle);
            }
        }

        void LoadBlips(Map map, XDocument xdoc)
        {
            var blips = xdoc?.Root.Element("Metadata")?.Elements("Blip");
            if (blips == null)
            {
                return;
            }
            foreach (var element in blips)
            {
                var position = element.ToVector3f() ?? Vector3f.One;
                var sprite = element.Element("Sprite")?.Value?.AsInt() ?? 0;
                var color = element.Element("Color")?.Value?.AsInt() ?? 0;
                var blip = new MapBlip(position, sprite, color);

                map.AddBlip(blip);
            }
        }

        static IEnumerable<XElement> SelectMapObjectsByType(XDocument xdoc, string type)
        {
            var mapObjects = xdoc?.Root?.Elements("Objects")?.Elements("MapObject");
            if (mapObjects == null)
            {
                yield break;
            }
            foreach (var mapObject in mapObjects)
            {
                if (string.Compare(mapObject.GetSubElementValue("Type"), type, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }
                yield return mapObject;
            }
        }

        void LoadMapPoints(Map map, XDocument xdoc)
        {
            LoadMapPoints(map, xdoc.Root?.Element("Metadata")?.Elements("LoadingPoint"));
            LoadMapPoints(map, xdoc.Root?.Element("Metadata")?.Elements("TeleportPoint"));
            LoadMapPoints(map, xdoc.Root?.Element("Metadata")?.Elements("NewPlayerSpawnPoint"));
            LoadMapPoints(map, xdoc.Root?.Element("Metadata")?.Elements("NamedPoint"));
        }

        static void LoadMapPoints(Map map, IEnumerable<XElement> elements)
        {
            if (elements == null)
            {
                return;
            }
            foreach (var metapoint in elements)
            {
                var mapPointType = metapoint.ToMapPointType() ?? MapPointType.None;
                var id = metapoint.Element("id")?.Value?.AsInt() ?? 0;
                var position = metapoint.ToVector3f() ?? Vector3f.One;
                var rotation = metapoint.Element("Rotation")?.ToVector3f();
                var name = metapoint.Element("Name")?.Value?.AsString();

                var mapPoint = new MapPoint(mapPointType, id, name, position, rotation);
                map.AddPoint(mapPoint);
            }
        }

        void LoadMarkers(Map map, XDocument xdoc)
        {
            var markers = xdoc.Root?.Element("Markers")?.Elements("Marker");
            if (markers == null)
            {
                return;
            }
            foreach (var marker in markers)
            {
                var markerType = marker.Element("Type")?.ToMarkerType() ?? MarkerType.UpsideDownCone;
                var position = marker.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var direction = marker.Element("Direction")?.ToVector3f() ?? Vector3f.One;
                var rotation = marker.Element("Rotation")?.ToVector3f() ?? Vector3f.One;
                var scale = marker.Element("Scale")?.ToVector3f() ?? Vector3f.One;
                var alpha = marker.Element("Alpha")?.Value?.AsByte() ?? 0;
                var r = marker.Element("Red")?.Value?.AsByte() ?? 0;
                var g = marker.Element("Green")?.Value?.AsByte() ?? 0;
                var b = marker.Element("Blue")?.Value?.AsByte() ?? 0;

                var mapMarker = new MapMarker(markerType, position, direction, rotation, scale, alpha, r, b, g);
                map.AddMarker(mapMarker);
            }
        }
    }

    internal static class MapLoaderXLinqExtensions
    {
        internal static MarkerType? ToMarkerType(this XElement element)
        {
            var v = element?.Value;
            if (v == null)
            {
                return null;
            }
            MarkerType markerType;
            if (Enum.TryParse(v, out markerType))
            {
                return markerType;
            }
            return null;
        }

        internal static MapObjectType? ToObjectType(this string objectTypeValue)
        {
            MapObjectType mapObjectType;
            if (Enum.TryParse(objectTypeValue, out mapObjectType))
            {
                return mapObjectType;
            }
            return null;
        }

        internal static MapPointType? ToMapPointType(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            switch (element.Name.LocalName.ToUpperInvariant())
            {
                case "LOADINGPOINT":
                    return MapPointType.Loading;
                case "TELEPORTPOINT":
                    return MapPointType.Teleport;
                case "NAMEDPOINT":
                    return MapPointType.Named;
            }

            MapPointType mapPointType;
            if (Enum.TryParse(element.Name.LocalName, out mapPointType))
            {
                return mapPointType;
            }
            return null;
        }

        internal static Vector3f? ToVector3f(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            var x = element?.Element("X")?.Value?.AsFloat() ?? 0;
            var y = element?.Element("Y")?.Value?.AsFloat() ?? 0;
            var z = element?.Element("Z")?.Value?.AsFloat() ?? 0;
            return new Vector3f(x, y, z);
        }

        internal static Color? ToColor(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            var a = element?.Element("Alpha")?.Value?.AsByte() ?? 0;
            var r = element?.Element("Red")?.Value?.AsByte() ?? 0;
            var g = element?.Element("Green")?.Value?.AsByte() ?? 0;
            var b = element?.Element("Blue")?.Value?.AsByte() ?? 0;
            return new Color(r, g, b, a);
        }

        internal static string GetSubElementValue(this XElement element, XName subElementName)
        {
            var el = element?.Element(subElementName);
            if (el != null)
            {
                return el.Value;
            }
            return null;
        }
    }
}
