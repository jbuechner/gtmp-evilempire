using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using gtmp.evilempire.entities;
using gtmp.evilempire.entities.customization;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.services
{
    class GtmpPlatformService : IPlatformService
    {
        public GtmpPlatformService(API api)
        {

        }

        public FreeroamCustomizationData GetFreeroamCharacterCustomizationData()
        {
            var data = new FreeroamCustomizationData();
            data.Models.Add(new FreeroamModel(Gender.Male, 1885233650, "Male"));
            data.Models.Add(new FreeroamModel(Gender.Male, -1667301416, "Female"));
            for (var i = 0; i < 46; i++)
            {
                data.Faces.Add(new FreeroamFace(i));
            }

            return data;
        }

        public CharacterCustomization GetDefaultCharacterCustomization()
        {
            return new CharacterCustomization
            {
                ModelHash = (int)PedHash.FreemodeMale01
            };
        }

        public void UpdateCharacterCustomizationOnClients(ISession session)
        {
            session.Client.CharacterModel = session.CharacterCustomization.ModelHash;
        }
    }
}
