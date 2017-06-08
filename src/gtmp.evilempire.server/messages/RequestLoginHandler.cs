using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.server.messages.transfer;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.messages
{
    class RequestLoginHandler : MessageHandlerBase
    {
        IAuthenticationService authentication;
        ISerializationService serialization;
        ICharacterService characters;
        ISessionStateTransitionService sessionStateTransition;
        ISessionService sessions;
        IPlatformService platform;
        IItemService items;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestLogin;
            }
        }

        public RequestLoginHandler(ServiceContainer services)
            : base(services)
        {
            authentication = services.Get<IAuthenticationService>();
            serialization = services.Get<ISerializationService>();
            characters = services.Get<ICharacterService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();
            platform = services.Get<IPlatformService>();
            sessions = services.Get<ISessionService>();
            items = services.Get<IItemService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var login = args.At(0).AsString();
            var password = args.At(1).AsString();

            var user = authentication.Authenticate(session, login, password);
            var client = session.Client;
            session.User = user;

            if (user == null)
            {
                client.TriggerClientEvent(ClientEvents.RequestLoginResponse, user != null);
            }
            else
            {
                var character = characters.GetActiveCharacter(session);
                session.Character = character;

                var characterCustomization = characters.GetCharacterCustomizationById(character.Id);
                if (characterCustomization == null)
                {
                    characterCustomization = characters.CreateDefaultCharacterCustomization(character.Id);
                }
                session.CharacterCustomization = characterCustomization;

                var characterInventory = characters.GetCharacterInventoryById(character.Id);
                if (characterInventory == null)
                {
                    characterInventory = characters.CreateDefaultCharacterInventory(character.Id);
                }
                session.CharacterInventory = characterInventory;
                sessions.SendMoneyChangedEvents(session);

                platform.UpdateCharacterCustomization(session);

                var response = new RequestLoginResponse
                {
                    User = new ClientUser(user),
                    Character = new ClientCharacter(character),
                    CharacterCustomization = new ClientCharacterCustomization(characterCustomization),
                    ItemDescriptions = items.GetItemDescriptions().Select(s => new ClientItemDescription(s))
                };
                var data = serialization.SerializeAsDesignatedJson(response);
                client.TriggerClientEvent(ClientEvents.RequestLoginResponse, user != null, data);

                sessionStateTransition.Transit(session, SessionState.LoggedIn);
            }

            return user != null;
        }
    }
}
