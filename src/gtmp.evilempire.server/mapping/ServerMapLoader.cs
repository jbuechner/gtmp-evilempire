using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Shared;
using System;
using static System.FormattableString;

namespace gtmp.evilempire.server.mapping
{
    class ServerMapLoader
    {
        public static void Load(Map map, API api)
        {
            foreach(var marker in map.Markers)
            {
                api.createMarker((int)marker.Type, marker.Position, marker.Direction, marker.Rotation, marker.Scale, marker.Alpha, marker.Red, marker.Green, marker.Blue);
            }
            foreach(var obj in map.Objects)
            {
                api.createObject(obj.Hash, obj.Position, obj.Rotation);
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
                var r = api.createPed(hash, ped.Position, ped.Rotation);
                r.invincible = ped.IsInvincible;
            }
            foreach(var vehicle in map.Vehicles)
            {
                if (!Enum.IsDefined(typeof(VehicleHash), vehicle.Hash))
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine(Invariant($"Invalid vehicle hash {vehicle.Hash}. Skipped."));
                    }
                    continue;
                }
                var hash = (VehicleHash)vehicle.Hash;
                var r = api.createVehicle(hash, vehicle.Position, vehicle.Rotation, vehicle.Color1, vehicle.Color2);
            }
        }
    }
}
