using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.server.messages.transfer;

namespace gtmp.evilempire.server.messages
{
    class RequestLoginHandler : MessageHandlerBase
    {
        IAuthenticationService authentication;
        ISerializationService serialization;
        ICharacterService characters;
        ISessionStateTransitionService sessionStateTransition;
        IPlatformService platform;

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
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var login = args.At(0).AsString();
            var password = args.At(1).AsString();

            var user = authentication.Authenticate(session, login, password);
            session.User = user;

            if (user == null)
            {
                session.Client.TriggerClientEvent(ClientEvents.RequestLoginResponse, user != null);
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

                platform.UpdateCharacterCustomizationOnClients(session);

                var response = new RequestLoginResponse
                {
                    User = new ClientUser(user),
                    Character = new ClientCharacter(character),
                    CharacterCustomization = new ClientCharacterCustomization(characterCustomization)
                };
                var data = serialization.SerializeAsDesignatedJson(response);
                session.Client.TriggerClientEvent(ClientEvents.RequestLoginResponse, user != null, data);

                sessionStateTransition.Transit(session, SessionState.LoggedIn);
            }

            return user != null;
        }
    }
}
