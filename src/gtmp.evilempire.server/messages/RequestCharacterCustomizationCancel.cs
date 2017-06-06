using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages
{
    class RequestCharacterCustomizationCancel : MessageHandlerBase
    {
        IPlatformService platform;
        ISessionStateTransitionService sessionStateTransition;
        ICharacterService characters;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestCustomizeCharacterCancel;
            }
        }

        public RequestCharacterCustomizationCancel(ServiceContainer services)
            : base(services)
        {
            platform = services.Get<IPlatformService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();
            characters = services.Get<ICharacterService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            if (session.State != SessionState.CharacterCustomization)
            {
                return false;
            }

            session.CharacterCustomization = characters.GetCharacterCustomizationById(session.Character.Id);
            sessionStateTransition.Transit(session, SessionState.Freeroam);
            session.State = SessionState.Freeroam;
            return true;
        }
    }
}
