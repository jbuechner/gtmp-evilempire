using GrandTheftMultiplayer.Server.Constant;
using gtmp.evilempire.entities;
using gtmp.evilempire.server.character.customization;

namespace gtmp.evilempire.server
{
    class PlatformService
    {
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

        public CharacterCustomization GetDefaultCharacterCustomization(int characterId)
        {
            return new CharacterCustomization
            {
                CharacterId = characterId,
                ModelHash = (int)PedHash.FreemodeMale01
            };
        }
    }
}
