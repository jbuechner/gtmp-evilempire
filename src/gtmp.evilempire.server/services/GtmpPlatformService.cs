using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.entities;
using gtmp.evilempire.entities.customization;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.services
{
    class GtmpPlatformService : IPlatformService
    {
        API api;

        public GtmpPlatformService(API api)
        {
            this.api = api;
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
            var defaultValue = new CharacterCustomization
            {
                ModelHash = (int)PedHash.FreemodeMale01
            };
            defaultValue.Face.ShapeMix = defaultValue.Face.SkinMix = 0.5f;

            return defaultValue;
        }

        public void UpdateCharacterCustomizationOnClients(ISession session)
        {
            var client = session.Client;
            var characterCustomization = session.CharacterCustomization;
            var face = characterCustomization.Face;

            var nativeClient = (Client)client.PlatformObject;

            api.setPlayerSkin(nativeClient, (PedHash)characterCustomization.ModelHash);
            api.sendNativeToAllPlayers(0x9414E18B9434C2FE, nativeClient, face.ShapeFirst, face.ShapeSecond, 0, face.SkinFirst, face.SkinSecond, 0, face.ShapeMix, face.SkinMix, 0f, false);
        }

        public void UpdateCharacterCustomization(ISession session)
        {
            var client = session.Client;
            var characterCustomization = session.CharacterCustomization;
            var face = characterCustomization.Face;

            var nativeClient = (Client)client.PlatformObject;

            api.setPlayerSkin(nativeClient, (PedHash)characterCustomization.ModelHash);
            api.sendNativeToPlayer(nativeClient, 0x9414E18B9434C2FE, nativeClient, face.ShapeFirst, face.ShapeSecond, 0, face.SkinFirst, face.SkinSecond, 0, face.ShapeMix, face.SkinMix, 0f, false);
        }
    }
}
