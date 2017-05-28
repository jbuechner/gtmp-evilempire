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
                { "FACE::SHAPEFIRST", UpdateFaceFirstShape }
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

                if (changed)
                {
                    platform.UpdateCharacterCustomization(session);
                    return SendCharacterCustomizationResponse(session, true, characterCustomization);
                }
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
            if (availableOptions.Faces.Any(p => p.Id == newValue))
            {
                characterCustomization.Face.ShapeFirst = newValue;
                return true;
            }
            return false;
        }
    }
}
