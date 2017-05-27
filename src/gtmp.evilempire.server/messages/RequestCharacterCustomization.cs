using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.messages
{
    class RequestCharacterCustomization : MessageHandlerBase
    {
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

                var availableOptions = platform.GetFreeroamCharacterCustomizationData();
                switch(what.ToUpperInvariant())
                {
                    case "MODEL":
                        if (availableOptions.Models.Any(p => p.Hash == value))
                        {
                            session.Client.CharacterModel = session.CharacterCustomization.ModelHash = value.Value;
                            return SendCharacterCustomizationResponse(session, true, session.CharacterCustomization);
                            // store into db after customization is finished
                        }
                        break;
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
    }
}
