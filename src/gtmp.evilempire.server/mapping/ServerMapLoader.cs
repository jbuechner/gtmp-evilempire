using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using gtmp.evilempire.services;
using System;
using System.Threading;
using static System.FormattableString;

namespace gtmp.evilempire.server.mapping
{
    static class ServerMapLoader
    {
        static readonly int TimeBetweenObjectCreationInMs = 10;

        public static Ped Load(MapPed ped, ServerAPI api)
        {
            if (!Enum.IsDefined(typeof(PedHash), ped.Hash))
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine(Invariant($"Invalid ped hash {ped.Hash}. Skipped."));
                }
            }
            var entity = api.createPed((PedHash)ped.Hash, ped.Position.ToVector3(), ped.Rotation);
            entity.invincible = ped.IsInvincible;
            entity.freezePosition = ped.IsPositionFrozen;
            entity.collisionless = ped.IsCollisionless;

            if (ped.Dialogue != null)
            {
                api.setEntitySyncedData(entity, "DIALOGUE:NAME", ped.Dialogue.Name);
            }
            if (ped.Title != null)
            {
                api.setEntitySyncedData(entity, "ENTITY:TITLE", ped.Title);
            }
            api.setEntitySyncedData(entity, "ENTITY:NET", entity.handle.Value);

            return entity;
        }

        public static GrandTheftMultiplayer.Server.Elements.Object Load(MapProp prop, ServerAPI api)
        {
            var entity = api.createObject(prop.Hash, prop.Position.ToVector3(), prop.Rotation.ToVector3());
            entity.collisionless = prop.IsCollisionless;
            entity.freezePosition = prop.IsPositionFrozen;
            return entity;
        }

        public static void Load(Map map, IPlatformService platform, API api)
        {
            Thread.Sleep(TimeBetweenObjectCreationInMs);

            foreach (var marker in map.Markers)
            {
                api.createMarker((int)marker.MarkerType, marker.Position.ToVector3(), marker.Direction.ToVector3(), marker.Rotation.ToVector3(), marker.Scale.ToVector3(), marker.Alpha, marker.Red, marker.Green, marker.Blue);
                Thread.Sleep(TimeBetweenObjectCreationInMs);
            }
            foreach(var obj in map.Props)
            {
                Load(obj, api);
                Thread.Sleep(TimeBetweenObjectCreationInMs);
            }
            foreach(var ped in map.Peds)
            {
                var entity = Load(ped, api);
                map.MakeAssociation(entity.handle.Value, ped);
                Thread.Sleep(TimeBetweenObjectCreationInMs);
            }
            foreach(var vehicle in map.Vehicles)
            {
                platform.SpawnVehicle(vehicle);
                Thread.Sleep(TimeBetweenObjectCreationInMs);
            }
            foreach (var blip in map.Blips)
            {
                var entity = api.createBlip(blip.Position.ToVector3());
                entity.color = blip.Color;
                entity.sprite = blip.Sprite;
                api.setBlipName(entity, blip.Name);
                Thread.Sleep(TimeBetweenObjectCreationInMs);
            }
        }
    }
}
