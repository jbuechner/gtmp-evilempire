using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.entities;
using gtmp.evilempire.entities.customization;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System.Collections.Generic;
using System.Linq;

namespace gtmp.evilempire.server.services
{
    class GtmpPlatformService : IPlatformService
    {
        readonly FreeroamCustomizationData freeroamCustomizationData;

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

            SendCharacterCustomizationToClient(nativeClient, nativeClient, characterCustomization);
        }

        void SendCharacterCustomizationToClient(Client recipient, Client characterClient, CharacterCustomization characterCustomization)
        {
            var face = characterCustomization.Face;

            api.setPlayerSkin(characterClient, (PedHash)characterCustomization.ModelHash);
            api.sendNativeToPlayer(recipient, 0x9414E18B9434C2FE, characterClient.handle, face.ShapeFirst, face.ShapeSecond, 0, face.SkinFirst, face.SkinSecond, 0, face.ShapeMix, face.SkinMix, 0f, false);
            api.sendNativeToPlayer(recipient, 0x262B14F48D29DE80, characterClient.handle, 2, characterCustomization.HairStyleId, 0, 0);
            api.sendNativeToPlayer(recipient, 0x4CFFC65454C93A49, characterClient.handle, characterCustomization.HairColorId, 0);
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
