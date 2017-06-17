using gtmp.evilempire.entities;
using gtmp.evilempire.entities.customization;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IPlatformService
    {
        FreeroamCustomizationData GetFreeroamCharacterCustomizationData();
        CharacterCustomization GetDefaultCharacterCustomization();

        void UpdateCharacterCustomization(ISession session);

        void SpawnPed(object ped); // todo: introduce ped poco/entity

        void SpawnVehicle(Vehicle vehicle);

        string GetVehicleModelName(Vehicle vehicle);

        bool IsClearRange(Vector3f point, float range, float height);

        object GetRuntimeEntityById(int id);

        object GetRuntimePedById(int id); // todo: introduce ped poco/entity
        Vehicle GetRuntimeVehicleById(int id);

        void UpdateSpawnedVehicle(Vehicle vehicle);

        void UpdateSpawnedPlayer(ISession session);

        bool IsInVehicle(ISession session, Vehicle vehicle);
    }
}
