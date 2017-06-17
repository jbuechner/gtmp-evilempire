using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using gtmp.evilempire.entities;
using gtmp.evilempire.entities.customization;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

using PlatformVehicle = GrandTheftMultiplayer.Server.Elements.Vehicle;
using ManagedVehicle = gtmp.evilempire.entities.Vehicle;

namespace gtmp.evilempire.server.services
{
    class GtmpPlatformService : IPlatformService
    {
        static class SynchronizationConstants
        {
            public static readonly string EntityType = "ENTITY_TYPE";
            public static readonly string EntityId = "ENTITY_ID";
            public static readonly string EntityDisplayName = "ENTITY_DISPLAYNAME";
            public static readonly string EntityDialogueName = "ENTITY_DIALOGUENAME";
            public static readonly string FaceShapeFirst = "FACE::SHAPEFIRST";
            public static readonly string FaceShapeSecond = "FACE::SHAPESECOND";
            public static readonly string FaceSkinFirst = "FACE::SKINFIRST";
            public static readonly string FaceSkinSecond = "FACE::SKINSECOND";
            public static readonly string FaceSkinMix = "FACE::SKINMIX";
            public static readonly string FaceShapeMix = "FACE::SHAPEMIX";
            public static readonly string FaceHairStyle = "HAIR::STYLE";
            public static readonly string FaceHairColor = "HAIR::COLOR";
        }

        readonly FreeroamCustomizationData freeroamCustomizationData;

        readonly IDictionary<int, MapPed> RuntimePedsMap = new Dictionary<int, MapPed>();
        readonly IDictionary<int, ManagedVehicle> RuntimeVehiclesMap = new Dictionary<int, ManagedVehicle>();
        readonly IDictionary<ManagedVehicle, PlatformVehicle> VehicleMap = new Dictionary<ManagedVehicle, PlatformVehicle>();

        readonly InMemorySequence EntityIdSequence = new InMemorySequence();

        API api;

        public GtmpPlatformService(API api)
        {
            this.api = api;

            freeroamCustomizationData = CreateFreeroamCustomizationData();
        }

        public FreeroamCustomizationData GetFreeroamCharacterCustomizationData()
        {
            return freeroamCustomizationData;
        }

        public CharacterCustomization GetDefaultCharacterCustomization()
        {
            var defaultValue = new CharacterCustomization
            {
                ModelHash = (int)PedHash.FreemodeMale01
            };
            defaultValue.Face.ShapeMix = defaultValue.Face.SkinMix = 0.5f;

            return defaultValue;
        }

        public void UpdateCharacterCustomization(ISession session)
        {
            var client = session.Client;
            var characterCustomization = session.CharacterCustomization;
            var nativeClient = (Client)client.PlatformObject;
            var face = characterCustomization.Face;

            nativeClient.setSyncedData(SynchronizationConstants.FaceShapeFirst, face.ShapeFirst);
            nativeClient.setSyncedData(SynchronizationConstants.FaceShapeSecond, face.ShapeSecond);
            nativeClient.setSyncedData(SynchronizationConstants.FaceSkinFirst, face.SkinFirst);
            nativeClient.setSyncedData(SynchronizationConstants.FaceSkinSecond, face.SkinSecond);
            nativeClient.setSyncedData(SynchronizationConstants.FaceSkinMix, face.SkinMix);
            nativeClient.setSyncedData(SynchronizationConstants.FaceShapeMix, face.ShapeMix);
            nativeClient.setSyncedData(SynchronizationConstants.FaceHairStyle, characterCustomization.HairStyleId);
            nativeClient.setSyncedData(SynchronizationConstants.FaceHairColor, characterCustomization.HairColorId);

            api.setPlayerDefaultClothes(nativeClient);
            api.setPlayerSkin(nativeClient, (PedHash)characterCustomization.ModelHash);
            api.sendNativeToPlayer(nativeClient, 0x9414E18B9434C2FE, nativeClient.handle, face.ShapeFirst, face.ShapeSecond, 0, face.SkinFirst, face.SkinSecond, 0, face.ShapeMix, face.SkinMix, 0f, false);
            api.sendNativeToPlayer(nativeClient, 0x262B14F48D29DE80, nativeClient.handle, 2, characterCustomization.HairStyleId, 0, 0);
            api.sendNativeToPlayer(nativeClient, 0x4CFFC65454C93A49, nativeClient.handle, characterCustomization.HairColorId, 0);
        }

        public string GetVehicleModelName(ManagedVehicle vehicle)
        {
            return api.getVehicleDisplayName((VehicleHash)vehicle.Hash);
        }

        public void SpawnPed(object ped)
        {
            MapPed mapPed = ped as MapPed;
            if (mapPed == null)
            {
                throw new ArgumentNullException(nameof(MapPed));
            }

            if (!Enum.IsDefined(typeof(PedHash), mapPed.Hash))
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine(Invariant($"Invalid ped hash {mapPed.Hash}. Skipped."));
                }
            }
            var entity = api.createPed((PedHash)mapPed.Hash, mapPed.Position.ToVector3(), mapPed.Rotation);
            var entityId = EntityIdSequence.Next();
            RuntimePedsMap.Add(entityId, mapPed);
            entity.invincible = mapPed.IsInvincible;
            entity.freezePosition = mapPed.IsPositionFrozen;
            entity.collisionless = mapPed.IsCollisionless;

            if (mapPed.Dialogue != null)
            {
                api.setEntitySyncedData(entity, SynchronizationConstants.EntityDialogueName, mapPed.Dialogue.Name);
            }
            if (mapPed.Title != null)
            {
                api.setEntitySyncedData(entity, SynchronizationConstants.EntityDisplayName, mapPed.Title);
            }
            api.setEntitySyncedData(entity, SynchronizationConstants.EntityType, (int)EntityType.Ped);
            api.setEntitySyncedData(entity, SynchronizationConstants.EntityId, entityId);
        }

        public void SpawnVehicle(ManagedVehicle vehicle)
        {
            if (!Enum.IsDefined(typeof(VehicleHash), vehicle.Hash))
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine(Invariant($"Invalid vehicle hash {vehicle.Hash}. Skipped."));
                }
            }
            var hash = (VehicleHash)vehicle.Hash;
            var entity = api.createVehicle(hash, vehicle.Position.ToVector3(), vehicle.Rotation.ToVector3(), vehicle.Color1, vehicle.Color2);
            var entityId = EntityIdSequence.Next();
            RuntimeVehiclesMap.Add(entityId, vehicle);
            VehicleMap.Add(vehicle, entity);

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

            api.setEntitySyncedData(entity, SynchronizationConstants.EntityType, (int)EntityType.Vehicle);
            api.setEntitySyncedData(entity, SynchronizationConstants.EntityId, entityId);
        }

        public object GetRuntimeEntityById(int id)
        {
            ManagedVehicle vehicle = GetRuntimeVehicleById(id);
            if (vehicle != null)
            {
                return vehicle;
            }
            object ped = GetRuntimePedById(id);
            if (ped != null)
            {
                return ped;
            }
            return null;
        }

        public object GetRuntimePedById(int id)
        {
            MapPed mapPed;
            if (RuntimePedsMap.TryGetValue(id, out mapPed))
            {
                return mapPed;
            }

            return null;
        }

        public ManagedVehicle GetRuntimeVehicleById(int id)
        {
            ManagedVehicle vehicle;
            if (RuntimeVehiclesMap.TryGetValue(id, out vehicle))
            {
                return vehicle;
            }
            return null;
        }

        public void UpdateSpawnedVehicle(ManagedVehicle vehicle)
        {
            PlatformVehicle entity;
            if (!VehicleMap.TryGetValue(vehicle, out entity) || entity == null)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[UpdateSpawnedVehicle] There is no spawned vehicle for {vehicle.Id}.");
                }
                return;
            }

            entity.locked = vehicle.IsLocked;
            entity.engineStatus = vehicle.IsEngineRunning;
        }

        public void UpdateSpawnedPlayer(ISession session)
        {
            Client nativeClient = session?.Client?.PlatformObject as Client;
            if (nativeClient == null)
            {
                return;
            }
            nativeClient.setSyncedData(SynchronizationConstants.EntityType, (int)EntityType.Player);
            nativeClient.setSyncedData(SynchronizationConstants.EntityId, EntityIdSequence.Next());
        }

        public bool IsClearRange(Vector3f point, float range, float height)
        {
            var vehicles = api.getAllVehicles();
            if (vehicles != null)
            {
                foreach(var vehicle in vehicles)
                {
                    var pos = api.getEntityPosition(vehicle);
                    if (Math.IsPointInSphere(point, range, pos.ToVector3f()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsInVehicle(ISession session, ManagedVehicle vehicle)
        {
            PlatformVehicle native;

            var nativeClient = session?.Client?.PlatformObject as Client;

            if (nativeClient != null && VehicleMap.TryGetValue(vehicle, out native) && native != null)
            {
                return nativeClient.isInVehicle && nativeClient.vehicle == native;
            }
            return false;
        }

        static FreeroamCustomizationData CreateFreeroamCustomizationData()
        {
            var data = new FreeroamCustomizationData();
            data.Models.Add(new FreeroamModel(Gender.Male, 1885233650, "Male"));
            data.Models.Add(new FreeroamModel(Gender.Male, -1667301416, "Female"));
            for (var i = 0; i < 46; i++)
            {
                data.Faces.Add(new FreeroamFace(i));
            }

            for (var i = 0; i < 37; i++)
            {
                data.HairStyles.Add(new FreeroamHairStyle(Gender.Male, i) { AvailableDuringCharacterCustomization = true });
            }
            for (var i = 0; i < 39; i++)
            {
                data.HairStyles.Add(new FreeroamHairStyle(Gender.Female, i) { AvailableDuringCharacterCustomization = true });
            }
            data.HairStyles.First(p => p.Gender == Gender.Male && p.Id == 23).AvailableDuringCharacterCustomization = false;
            data.HairStyles.First(p => p.Gender == Gender.Female && p.Id == 24).AvailableDuringCharacterCustomization = false;

            for (var i = 0; i < 64; i++)
            {
                data.HairColors.Add(new FreeroamHairColor(i));
            }

            return data;
        }
    }
}
