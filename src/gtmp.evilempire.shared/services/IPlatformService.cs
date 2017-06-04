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

        void SpawnVehicle(Vehicle vehicle);

        bool IsClearRange(Vector3f point, float range, float height);
    }
}
