using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.entities;
using gtmp.evilempire.entities.customization;

namespace gtmp.evilempire.server.messages
{
    class RequestCharacterCustomization : MessageHandlerBase
    {
        delegate bool UpdateCharacterCustomization(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue);

        readonly IDictionary<string, UpdateCharacterCustomization> handlers;

        IPlatformService platform;
        ISerializationService serialization;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestCustomizeCharacter;
            }
        }

        public RequestCharacterCustomization(ServiceContainer services)
            : base(services)
        {
            platform = services.Get<IPlatformService>();
            serialization = services.Get<ISerializationService>();

            handlers = new Dictionary<string, UpdateCharacterCustomization>
            {
                { "MODEL", UpdateModel },
                { "FACE::SHAPEFIRST", UpdateFaceFirstShape },
                { "FACE::SHAPESECOND", UpdateFaceSecondShape },
                { "FACE::SKINFIRST", UpdateFaceFirstSkin },
                { "FACE::SKINSECOND", UpdateFaceSecondSkin },
                { "FACE::SKINMIX", UpdateFaceSkinMix },
                { "FACE::SHAPEMIX", UpdateFaceShapeMix }
            };
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            if (session.State == SessionState.CharacterCustomization &&  !session.Character.HasBeenThroughInitialCustomization)
            {
                var what = args.At(0).AsString();
                var value = args.At(1).AsInt();

                if (what == null || !value.HasValue)
                {
                    return SendCharacterCustomizationResponse(session, false, null);
                }

                var changed = false;
                var characterCustomization = session.CharacterCustomization;

                var availableOptions = platform.GetFreeroamCharacterCustomizationData();

                UpdateCharacterCustomization handler;
                if (handlers.TryGetValue(what.ToUpperInvariant(), out handler))
                {
                    changed = handler?.Invoke(availableOptions, characterCustomization, value.Value) ?? false;
                }

                platform.UpdateCharacterCustomization(session);
                return SendCharacterCustomizationResponse(session, changed, characterCustomization);
            }

            return SendCharacterCustomizationResponse(session, false, null);
        }

        bool SendCharacterCustomizationResponse(ISession session, bool success, CharacterCustomization characterCustomization)
        {
            if (characterCustomization != null)
            {
                var response = new RequestCharacterCustomizationResponse
                {
                    CharacterCustomization = new transfer.ClientCharacterCustomization(characterCustomization)
                };
                var data = serialization.SerializeAsDesignatedJson(response);
                session.Client.TriggerClientEvent(ClientEvents.RequestCustomizeCharacterResponse, success, data);
            }
            else
            {
                session.Client.TriggerClientEvent(ClientEvents.RequestCustomizeCharacterResponse, success);
            }
            return success;
        }

        static bool UpdateModel(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (availableOptions.Models.Any(p => p.Hash == newValue))
            {
                characterCustomization.ModelHash = newValue;
                return true;
            }
            return false;
        }

        static bool UpdateFaceFirstShape(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (IsFaceAvailable(availableOptions, newValue))
            {
                characterCustomization.Face.ShapeFirst = newValue;
                return true;
            }
            return false;
        }

        static bool UpdateFaceSecondShape(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (IsFaceAvailable(availableOptions, newValue))
            {
                characterCustomization.Face.ShapeSecond = newValue;
                return true;
            }
            return false;
        }

        static bool UpdateFaceFirstSkin(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (IsFaceAvailable(availableOptions, newValue))
            {
                characterCustomization.Face.SkinFirst = newValue;
                return true;
            }
            return false;
        }

        static bool UpdateFaceSecondSkin(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (IsFaceAvailable(availableOptions, newValue))
            {
                characterCustomization.Face.SkinSecond = newValue;
                return true;
            }
            return false;
        }

        static bool UpdateFaceSkinMix(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (IsValidMixValue(newValue))
            {
                characterCustomization.Face.SkinMix = newValue / 100f;
                return true;
            }

            return false;
        }

        static bool UpdateFaceShapeMix(FreeroamCustomizationData availableOptions, CharacterCustomization characterCustomization, int newValue)
        {
            if (IsValidMixValue(newValue))
            {
                characterCustomization.Face.ShapeMix = newValue / 100f;
                return true;
            }

            return false;
        }

        static bool IsFaceAvailable(FreeroamCustomizationData availableOptions, int newValue)
        {
            return availableOptions.Faces.Any(p => p.Id == newValue);
        }

        static bool IsValidMixValue(int value)
        {
            return value >= 0 && value <= 100;
        }
    }
}
