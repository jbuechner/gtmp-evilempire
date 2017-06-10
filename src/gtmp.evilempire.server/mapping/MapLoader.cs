using gtmp.evilempire.entities;
using gtmp.evilempire.server.actions;
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
    class MapLoader
    {
        IList<Action<Map, XDocument>> Handlers { get; }

        public MapLoader()
        {
            Handlers = new List<Action<Map, XDocument>> { LoadMarkers, LoadMapPoints, LoadProps, LoadPeds, LoadVehicles, LoadBlips, LoadItemDescriptions, LoadMetas };
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
            ValidateMap(map);
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
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.Zero;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.Zero;

                var obj = new MapProp(templateName, hash, position, rotation);
                obj.IsPositionFrozen = mapObject.Element("IsPositionFrozen")?.Value?.AsBool() ?? false;
                obj.IsCollisionless = mapObject.Element("IsCollisionless")?.Value?.AsBool() ?? false;
                map.AddProp(obj);
            }
        }

        void LoadPeds(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Ped"))
            {
                var templateName = mapObject.Element("TemplateName")?.Value;
                var hash = mapObject.Element("Hash")?.Value?.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.Zero;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.Zero;
                var isInvincible = mapObject.Element("IsInvicible")?.Value?.AsBool() ?? false;

                var ped = new MapPed(templateName, hash, position, rotation.Z, isInvincible);
                ped.IsPositionFrozen = mapObject.Element("IsPositionFrozen")?.Value?.AsBool() ?? false;
                ped.IsCollisionless = mapObject.Element("IsCollisionless")?.Value?.AsBool() ?? false;
                ped.Title = mapObject.Element("Title")?.Value;

                var dialogueElement = mapObject.Element("Dialogue");
                if (dialogueElement != null)
                {
                    var dialogue = LoadMapDialogue(map, dialogueElement);
                    if (dialogue != null)
                    {
                        ped.Dialogue = dialogue;
                        map.AddDialogue(dialogue);
                    }
                }

                map.AddPed(ped);
            }
        }

        void LoadVehicles(Map map, XDocument xdoc)
        {
            foreach (var mapObject in SelectMapObjectsByType(xdoc, "Vehicle"))
            {
                var templateName = mapObject.Element("TemplateName")?.Value;
                var hash = mapObject.Element("Hash")?.Value?.AsInt() ?? 0;
                var position = mapObject.Element("Position")?.ToVector3f() ?? Vector3f.Zero;
                var rotation = mapObject.Element("Rotation")?.ToVector3f() ?? Vector3f.Zero;
                var color1 = mapObject.Element("PrimaryColor")?.Value?.AsInt() ?? 0;
                var color2 = mapObject.Element("SecondaryColor")?.Value?.AsInt() ?? 0;

                var vehicle = new Vehicle();
                vehicle.Hash = hash;
                vehicle.Position = position;
                vehicle.Rotation = rotation;
                vehicle.Color1 = color1;
                vehicle.Color2 = color2;
                vehicle.TemplateName = templateName;

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
                    List<Vehicle.Neon> neons = new List<Vehicle.Neon>();
                    foreach(var neonElement in neonElements)
                    {
                        var neonIndex = neonElement.Attribute("Slot")?.Value?.AsInt() ?? 0;
                        var isTurnedOn = neonElement.Attribute("On")?.Value?.AsBool() ?? false;
                        var neon = new Vehicle.Neon { Index = neonIndex, IsTurnedOn = isTurnedOn };
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
                var position = element.ToVector3f() ?? Vector3f.Zero;
                var sprite = element.Element("Sprite")?.Value?.AsInt() ?? 0;
                var color = element.Element("Color")?.Value?.AsInt() ?? 0;
                var name = element.Element("Name")?.Value;
                var isShortRange = element.Element("IsShortRange")?.Value.AsBool() ?? false;
                var blip = new MapBlip(position, sprite, color, isShortRange, name);

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
                var position = metapoint.ToVector3f() ?? Vector3f.Zero;
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
                var position = marker.Element("Position")?.ToVector3f() ?? Vector3f.Zero;
                var direction = marker.Element("Direction")?.ToVector3f() ?? Vector3f.Zero;
                var rotation = marker.Element("Rotation")?.ToVector3f() ?? Vector3f.Zero;
                var scale = marker.Element("Scale")?.ToVector3f() ?? Vector3f.Zero;
                var alpha = marker.Element("Alpha")?.Value?.AsByte() ?? 0;
                var r = marker.Element("Red")?.Value?.AsByte() ?? 0;
                var g = marker.Element("Green")?.Value?.AsByte() ?? 0;
                var b = marker.Element("Blue")?.Value?.AsByte() ?? 0;

                var mapMarker = new MapMarker(markerType, position, direction, rotation, scale, alpha, r, g, b);
                map.AddMarker(mapMarker);
            }
        }

        MapDialogue LoadMapDialogue(Map map, XElement dialogueElement)
        {
            if (dialogueElement == null)
            {
                return null;
            }
            var startDialoguePageKey = dialogueElement.Element("StartPage")?.Value;
            if (!string.IsNullOrEmpty(startDialoguePageKey))
            {
                var startDialoguePage = dialogueElement.Element("Pages")?.Elements("Page")?.Where(p => p != null && string.CompareOrdinal(p.Element("Name")?.Value, startDialoguePageKey) == 0).FirstOrDefault();
                var mapDialogue = ReadDialoguePage<MapDialogue>(null, startDialoguePage);
                ReadDialoguePages(mapDialogue, dialogueElement);
                return mapDialogue;
            }
            return null;
        }

        T ReadDialoguePage<T>(MapDialoguePage dialoguePage, XElement dialoguePageElement)
            where T : MapDialoguePage
        {
            if (dialoguePageElement == null)
            {
                throw new ArgumentNullException(nameof(dialoguePageElement));
            }
            var name = dialoguePageElement?.Element("Name")?.Value;
            var markdown = dialoguePageElement?.Element("Markdown")?.Value;
            var newDialoguePage = (T)Activator.CreateInstance(typeof(T), name, markdown);
            newDialoguePage.Actions = LoadActionSets(dialoguePageElement.Elements("Actions")).ToList();

            if (dialoguePage != null)
            {
                dialoguePage.Pages.Add(newDialoguePage);
            }
            ReadDialoguePages(newDialoguePage, dialoguePageElement);
            return newDialoguePage;
        }

        void ReadDialoguePages(MapDialoguePage dialoguePage, XElement dialoguePageElement)
        {
            if (dialoguePage == null)
            {
                throw new ArgumentNullException(nameof(dialoguePage));
            }
            if (dialoguePageElement == null)
            {
                throw new ArgumentNullException(nameof(dialoguePageElement));
            }
            var pageElements = dialoguePageElement?.Element("Pages")?.Elements("Page");
            if (pageElements != null)
            {
                foreach(var pageElement in pageElements)
                {
                    if (pageElement == null)
                    {
                        continue;
                    }
                    ReadDialoguePage<MapDialoguePage>(dialoguePage, pageElement);
                }
            }
        }

        void LoadItemDescriptions(Map map, XDocument doc)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var itemDescriptionsElement = doc.Root.Element("ItemDescriptions");
            if (itemDescriptionsElement == null)
            {
                return;
            }

            var itemDescriptionElements = itemDescriptionsElement.Elements("ItemDescription");
            if (itemDescriptionElements == null)
            {
                return;
            }
            foreach(var itemDescriptionElement in itemDescriptionElements)
            {
                if (itemDescriptionElement == null)
                {
                    continue;
                }

                var id = itemDescriptionElement.Element("Id")?.Value?.AsInt();
                var name = itemDescriptionElement.Element("Name")?.Value;
                var weight = itemDescriptionElement.Element("Weight")?.Value.AsDouble();
                var volume = itemDescriptionElement.Element("Volume")?.Value.AsDouble();
                var isStackable = itemDescriptionElement.Element("IsStackable")?.Value.AsBool();
                var maximumStack = itemDescriptionElement.Element("MaximumStack")?.Value.AsInt();
                var associateCurrencyAsRawEnumValue = itemDescriptionElement.Element("AssociatedCurrency")?.Value;
                var currencyDenomination = itemDescriptionElement.Element("Denomination")?.Value.AsDouble();
                var description = itemDescriptionElement.Element("Description")?.Value;

                Currency currency = Currency.None;
                if (!string.IsNullOrEmpty(associateCurrencyAsRawEnumValue) && !Enum.TryParse<Currency>(associateCurrencyAsRawEnumValue, out currency))
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"The associated currency {associateCurrencyAsRawEnumValue} is not a known member of {typeof(Currency).Name}. Item Id = {id}");
                        continue;
                    }
                }

                if (!id.HasValue)
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"No item id specified for the given item. Skipping.");
                        continue;
                    }
                }

                var itemDescription = new ItemDescription { Id = id.Value, Name = name, Volume = volume ?? 0, Weight = weight ?? 0, IsStackable = isStackable ?? false, MaximumStack = maximumStack ?? 1, AssociatedCurrency = currency, Denomination = currencyDenomination ?? 1 };
                itemDescription.Description = description;
                map.AddItemDescription(itemDescription);
            }
        }

        void LoadMetas(Map map, XDocument doc)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var items = doc.Root?.Element("Metadata")?.Element("StartingInventory")?.Element("Items")?.Elements("Item");
            foreach(var item in LoadItems(items))
            {
                map.Metadata.StartingInventoryItems.Add(item);
            }

            var timers = doc.Root?.Element("Metadata")?.Element("Timers")?.Elements("Timer");
            if (timers != null)
            {
                foreach(var timer in timers)
                {
                    if (timer == null)
                    {
                        continue;
                    }

                    var intervalValue = timer.Element("Interval")?.Value;
                    var interval = intervalValue?.AsTimeSpan();
                    if (!interval.HasValue)
                    {
                        using (ConsoleColor.Yellow.Foreground())
                        {
                            Console.WriteLine($"The map timer has an invalid interval and will be skipped. Raw value was {intervalValue}.");
                        }
                        continue;
                    }


                    items = timer.Element("Items")?.Elements("Item");
                    var mapTimer = new MapTimer { Interval = interval.Value };
                    mapTimer.Actions = LoadActionSets(timer.Elements("Actions")).ToList();
                    map.Metadata.Timers.Add(mapTimer);
                }
            }
        }

        static IEnumerable<Item> LoadItems(IEnumerable<XElement> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var itemDescriptionIdElement = item.Element("ItemDescriptionId");
                    var itemDescriptionId = itemDescriptionIdElement?.Value?.AsInt();
                    var amount = item.Element("Amount")?.Value?.AsInt() ?? 1;

                    if (!itemDescriptionId.HasValue)
                    {
                        using (ConsoleColor.Yellow.Foreground())
                        {
                            Console.WriteLine("The starting inventory item does not have a item description id. Skipping.");
                        }
                        continue;
                    }

                    yield return new Item { ItemDescriptionId = itemDescriptionId.Value, Amount = amount };
                }
            }
        }

        static IEnumerable<ActionSet> LoadActionSets(IEnumerable<XElement> elements)
        {
            if (elements == null)
            {
                yield break;
            }
            foreach(var element in elements)
            {
                var actionSet = new ActionSet();

                var ifElement = element?.Element("If");
                var thenElement = element?.Element("Then");
                var elseElement = element?.Element("Else");
                var actionElements = element?.Elements("Action");

                if (ifElement == null && actionElements != null)
                {
                    actionSet.Condition = ActionConditionalOperation.True;
                    foreach (var actionElement in actionElements)
                    {
                        var thenAction = LoadActionSetActions(actionElement);
                        actionSet.ThenActions.Add(thenAction);
                    }
                }
                else
                {
                    if (ifElement != null)
                    {
                        actionSet.Condition = LoadActionSetCondition(ifElement?.Element("Operation"));

                        foreach (var actionElement in element?.Element("Then")?.Elements("Action"))
                        {
                            var thenAction = LoadActionSetActions(actionElement);
                            actionSet.ThenActions.Add(thenAction);
                        }

                        foreach (var actionElement in element?.Element("Else")?.Elements("Action"))
                        {
                            var thenAction = LoadActionSetActions(actionElement);
                            actionSet.ElseActions.Add(thenAction);
                        }
                    }
                    else
                    {
                        using (ConsoleColor.Yellow.Foreground())
                        {
                            Console.WriteLine("[MapLoader] The action set uses an unsupported form. Either use Action elements directly beneath Actions or use If/Then/Else elements respectively.");
                        }
                    }
                }

                yield return actionSet;
            }
        }

        static ActionSetItem LoadActionSetActions(XElement actionElement)
        {
            var sequenceElement = actionElement?.Element("Sequence");
            if (sequenceElement == null)
            {
                return null;
            }

            var sequenceItemElements = sequenceElement.Elements("Item");
            if (sequenceItemElements == null)
            {
                return null;
            }
            var action = new ActionSetItem();
            foreach (var sequenceItemElement in sequenceItemElements)
            {
                var actionType = sequenceItemElement?.Attribute("Type")?.Value;
                var sequenceItem = new ActionSequenceItem { ActionType = actionType };
                action.Sequence.Items.Add(sequenceItem);

                var argumentElements = sequenceItemElement.Elements();
                if (argumentElements != null)
                {
                    foreach (var argumentElement in argumentElements)
                    {
                        if (argumentElement == null)
                        {
                            continue;
                        }
                        sequenceItem.Args[argumentElement.Name.LocalName] = argumentElement.Value;
                    }
                }

            }
            return action;
        }

        static ActionConditionalOperation LoadActionSetCondition(XElement element)
        {

            var comparatorElement = element.Attribute("Comparator")?.Value;
            var comparator = ActionConditionalComparator.GetComparator(comparatorElement);
            if (comparator == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[MapLoader] There is no comparator with the name \"{comparator}\".");
                }
                return null;
            }

            var operandA = LoadActionSetConditionOperand(element.Element("OperandA"));
            var operandB = LoadActionSetConditionOperand(element.Element("OperandB"));

            return new ActionConditionalOperation(comparator, operandA, operandB);
        }

        static ActionConditionalOperand LoadActionSetConditionOperand(XElement element)
        {
            var operationElement = element.Element("Operation");
            if (operationElement != null)
            {
                return LoadActionSetCondition(operationElement);
            }
            var constantElement = element.Element("Constant");
            if (constantElement != null)
            {
                return new ActionConditionalOperand.Constant(constantElement.Value);
            }
            var propertyElement = element.Element("Property");
            if (propertyElement != null)
            {
                return new ActionConditionalOperand.Property(propertyElement.Value);
            }

            using (ConsoleColor.Yellow.Foreground())
            {
                Console.WriteLine("[MapLoader] Unknown operation operand. Either expected a Property / Constant or another Operation element inside.");
            }
            return null;
        }

        static void ValidateMap(Map map)
        {
            var metadata = map.Metadata;
            var startingInventoryItems = metadata.StartingInventoryItems;
            ValidateItems(map, startingInventoryItems, "starting inventory");

            var timers = metadata.Timers;
            foreach(var timer in timers)
            {
                if (timer == null)
                {
                    continue;
                }
                if (timer.Interval.TotalMilliseconds < 1)
                {
                    Console.WriteLine($"[ValidateMap] A timers interval is less than one millisecond and will not be used.");
                    continue;
                }
                ValidateItems(map, startingInventoryItems, "timer");
            }
        }

        static void ValidateItems(Map map, IEnumerable<Item> items, string subject)
        {
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                ItemDescription itemDescription;
                if (!map.ItemDescriptionMap.TryGetValue(item.ItemDescriptionId, out itemDescription) || itemDescription == null)
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"[ValidateMap] The {subject} item with item description id {item.ItemDescriptionId} is invalid because there is not item description with the given id.");
                    }
                }
                if (item.Amount < 0)
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"[ValidateMap] The {subject} item with item description id {item.ItemDescriptionId} is invalid because the amount is negative.");
                    }
                }
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
