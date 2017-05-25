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
            return api.createVehicle(hash, vehicle.Position.ToVector3(), vehicle.Rotation.ToVector3(), vehicle.Color1, vehicle.Color2);
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
        }
    }
}
