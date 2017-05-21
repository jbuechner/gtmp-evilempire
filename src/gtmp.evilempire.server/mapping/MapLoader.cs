using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using System;
using System.Collections.Generic;
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
            Handlers = new List<Action<Map, XDocument>> { LoadMarkers, LoadMapPoints };
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

        void LoadMapPoints(Map map, XDocument xdoc)
        {
            LoadMapPoints(map, xdoc.Root?.Element("Metadata")?.Elements("LoadingPoint"));
            LoadMapPoints(map, xdoc.Root?.Element("Metadata")?.Elements("TeleportPoint"));
        }

        void LoadMapPoints(Map map, IEnumerable<XElement> elements)
        {
            if (elements == null)
            {
                return;
            }
            foreach (var metapoint in elements)
            {
                var mapPointType = metapoint.ToMapPointType() ?? MapPointType.None;
                var id = metapoint.Element("id").ToInt() ?? 0;
                var position = metapoint.ToVector3() ?? new Vector3();

                var mapPoint = new MapPoint(mapPointType, id, position);
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
                var position = marker.Element("Position")?.ToVector3() ?? new Vector3();
                var direction = marker.Element("Direction")?.ToVector3() ?? new Vector3();
                var rotation = marker.Element("Rotation")?.ToVector3() ?? new Vector3();
                var scale = marker.Element("Scale")?.ToVector3() ?? new Vector3(1, 1, 1);
                var alpha = marker.Element("Alpha").ToByte() ?? 0;
                var r = marker.Element("Red").ToByte() ?? 0;
                var g = marker.Element("Green").ToByte() ?? 0;
                var b = marker.Element("Blue").ToByte() ?? 0;

                var mapMarker = new MapMarker(markerType, position, direction, rotation, scale, alpha, r, g, b);
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
            }

            MapPointType mapPointType;
            if (Enum.TryParse(element.Name.LocalName, out mapPointType))
            {
                return mapPointType;
            }
            return null;
        }

        internal static Vector3 ToVector3(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            var x = element?.Element("X").ToFloat() ?? 0;
            var y = element?.Element("Y").ToFloat() ?? 0;
            var z = element?.Element("Z").ToFloat() ?? 0;
            return new Vector3(x, y, z);
        }

        internal static int? ToInt(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            int v;
            if (int.TryParse(element.Value, out v))
            {
                return v;
            }
            return null;
        }

        internal static byte? ToByte(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            byte v;
            if (byte.TryParse(element.Value, out v))
            {
                return v;
            }
            return null;
        }

        internal static float? ToFloat(this XElement element)
        {
            if (element == null)
            {
                return null;
            }
            float v;
            if (float.TryParse(element.Value, out v))
            {
                return v;
            }
            return null;
        }
    }
}
