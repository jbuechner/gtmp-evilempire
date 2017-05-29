using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using System;
using static System.FormattableString;

namespace gtmp.evilempire.server.mapping
{
    static class ServerMapLoader
    {
        public static Vehicle Load(MapVehicle vehicle, ServerAPI api)
        {
            if (!Enum.IsDefined(typeof(VehicleHash), vehicle.Hash))
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine(Invariant($"Invalid vehicle hash {vehicle.Hash}. Skipped."));
                }
                return null;
            }
            var hash = (VehicleHash)vehicle.Hash;
            var entity = api.createVehicle(hash, vehicle.Position.ToVector3(), vehicle.Rotation.ToVector3(), vehicle.Color1, vehicle.Color2);

            if (vehicle.NumberPlate != null)
            {
                entity.numberPlate = vehicle.NumberPlate;
            }
            if (vehicle.NumberPlateStyle.HasValue)
            {
                entity.numberPlateStyle = vehicle.NumberPlateStyle.Value;
            }
            entity.specialLight = vehicle.IsSpecialLightEnabled;
            if (vehicle.TrimColor.HasValue)
            {
                entity.trimColor = vehicle.TrimColor.Value;
            }

            if (vehicle.BrokenWindows != null)
            {
                foreach (var windowIndex in vehicle.BrokenWindows)
                {
                    entity.breakWindow(windowIndex);
                }
            }
            if (vehicle.OpenedDoors != null)
            {
                foreach (var doorIndex in vehicle.OpenedDoors)
                {
                    entity.openDoor(doorIndex);
                }
            }
            if (vehicle.BrokenDoors != null)
            {
                foreach (var doorIndex in vehicle.BrokenDoors)
                {
                    entity.breakDoor(doorIndex);
                }
            }
            if (vehicle.PoppedTyres != null)
            {
                foreach (var tyreIndex in vehicle.PoppedTyres)
                {
                    entity.popTyre(tyreIndex);
                }
            }
            if (vehicle.Neons != null)
            {
                foreach (var neon in vehicle.Neons)
                {
                    entity.setNeons(neon.Index, neon.IsTurnedOn);
                }
            }

            if (vehicle.EnginePowerMultiplier.HasValue)
            {
                entity.enginePowerMultiplier = vehicle.EnginePowerMultiplier.Value;
            }
            if (vehicle.EngineTorqueMultiplier.HasValue)
            {
                entity.engineTorqueMultiplier = vehicle.EngineTorqueMultiplier.Value;
            }

            if (vehicle.CustomPrimaryColor.HasValue)
            {
                entity.customPrimaryColor = vehicle.CustomPrimaryColor.Value.ToColor();
            }
            if (vehicle.CustomSecondaryColor.HasValue)
            {
                entity.customSecondaryColor = vehicle.CustomSecondaryColor.Value.ToColor();
            }
            if (vehicle.ModColor1.HasValue)
            {
                entity.modColor1 = vehicle.ModColor1.Value.ToColor();
            }
            if (vehicle.ModColor2.HasValue)
            {
                entity.modColor2 = vehicle.ModColor2.Value.ToColor();
            }
            if (vehicle.NeonColor.HasValue)
            {
                entity.neonColor = vehicle.NeonColor.Value.ToColor();
            }
            if (vehicle.TyreSmokeColor.HasValue)
            {
                entity.tyreSmokeColor = vehicle.TyreSmokeColor.Value.ToColor();
            }
            if (vehicle.WheelColor.HasValue)
            {
                entity.wheelColor = vehicle.WheelColor.Value;
            }
            if (vehicle.WheelType.HasValue)
            {
                entity.wheelType = vehicle.WheelType.Value;
            }
            if (vehicle.WindowTint.HasValue)
            {
                entity.windowTint = vehicle.WindowTint.Value;
            }
            if (vehicle.DashboardColor.HasValue)
            {
                entity.dashboardColor = vehicle.DashboardColor.Value;
            }
            if (vehicle.Health.HasValue)
            {
                entity.health = vehicle.Health.Value;
            }
            if (vehicle.Livery.HasValue)
            {
                entity.livery = vehicle.Livery.Value;
            }
            if (vehicle.PearlescentColor.HasValue)
            {
                entity.pearlescentColor = vehicle.PearlescentColor.Value;
            }

            entity.locked = vehicle.IsLocked;
            entity.freezePosition = vehicle.IsPositionFrozen;
            entity.engineStatus = vehicle.IsEngineRunning;
            entity.bulletproofTyres = vehicle.HasBulletproofTyres;
            entity.invincible = vehicle.IsInvincible;
            entity.collisionless = vehicle.IsCollisionless;

            return entity;
        }

        public static void Load(Map map, API api)
        {
            foreach(var marker in map.Markers)
            {
                api.createMarker((int)marker.MarkerType, marker.Position.ToVector3(), marker.Direction.ToVector3(), marker.Rotation.ToVector3(), marker.Scale.ToVector3(), marker.Alpha, marker.Red, marker.Green, marker.Blue);
            }
            foreach(var obj in map.Props)
            {
                api.createObject(obj.Hash, obj.Position.ToVector3(), obj.Rotation.ToVector3());
            }
            foreach(var ped in map.Peds)
            {
                if (!Enum.IsDefined(typeof(PedHash), ped.Hash))
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine(Invariant($"Invalid ped hash {ped.Hash}. Skipped."));
                    }
                    continue;
                }

                var hash = (PedHash)ped.Hash;
                var r = api.createPed(hash, ped.Position.ToVector3(), ped.Rotation);
                r.invincible = ped.IsInvincible;
            }
            foreach(var vehicle in map.Vehicles)
            {
                Load(vehicle, api);
            }

            foreach (var blip in map.Blips)
            {
                var entity = api.createBlip(blip.Position.ToVector3());
                entity.color = blip.Color;
                entity.sprite = blip.Sprite;
                api.setBlipName(entity, blip.Name);
            }
        }
    }
}
