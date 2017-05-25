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
            Handlers = new List<Action<Map, XDocument>> { LoadMarkers, LoadMapPoints, LoadObjects, LoadPeds, LoadVehicles };
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

        void LoadObjects(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Prop"))
            {
                var hash = mapObject.Element("Hash")?.Value.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.One;

                var obj = new MapObject(hash, position, rotation);
                map.AddObject(obj);
            }
        }

        void LoadPeds(Map map, XDocument xdoc)
        {
            foreach(var mapObject in SelectMapObjectsByType(xdoc, "Ped"))
            {
                var hash = mapObject.Element("Hash")?.Value?.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.One;
                var isInvincible = mapObject.Element("Invicible")?.Value?.AsBool() ?? false;

                var ped = new MapPed(hash, position, rotation.Z, isInvincible);
                map.AddPed(ped);
            }
        }

        void LoadVehicles(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Vehicle"))
            {
                var hash = mapObject.Element("Hash")?.Value?.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.One;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.One;
                var color1 = mapObject.Element("PrimaryColor")?.Value?.AsInt() ?? 0;
                var color2 = mapObject.Element("SecondaryColor")?.Value?.AsInt() ?? 0;

                var vehicle = new MapVehicle(hash, position, rotation, color1, color2);
                map.AddVehicle(vehicle);
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
                var name = metapoint.Element("Name")?.Value?.AsString();

                var mapPoint = new MapPoint(mapPointType, id, name, position);
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

        internal static MapPointType? ToMapPointType(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            switch(element.Name.LocalName.ToUpperInvariant())
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
