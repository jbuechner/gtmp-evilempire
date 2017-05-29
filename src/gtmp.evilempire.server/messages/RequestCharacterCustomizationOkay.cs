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
    class RequestCharacterCustomizationOkay : MessageHandlerBase
    {
        IPlatformService platform;
        ISessionStateTransitionService sessionStateTransition;
        IDbService db;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestCustomizeCharacterOk;
            }
        }

        public RequestCharacterCustomizationOkay(ServiceContainer services)
            : base(services)
        {
            platform = services.Get<IPlatformService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();
            db = services.Get<IDbService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            if (session.State != SessionState.CharacterCustomization)
            {
                return false;
            }

            session.Character.HasBeenThroughInitialCustomization = true;
            db.Update<Character>(session.Character);

            db.Update<CharacterCustomization>(session.CharacterCustomization);
            sessionStateTransition.Transit(session, SessionState.Freeroam);
            return true;
        }
    }
}
