using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;

namespace gtmp.evilempire.server.sessions
{
    class SessionCharacterCustomizationStateHandler : SessionStateHandlerBase
    {
        IPlatformService platform;
        ISerializationService serialization;

        public override SessionState HandledSessionState
        {
            get
            {
                return SessionState.CharacterCustomization;
            }
        }

        public SessionCharacterCustomizationStateHandler(ServiceContainer services)
            : base(services)
        {
            platform = services.Get<IPlatformService>();
            serialization = services.Get<ISerializationService>();
        }

        public override bool Transit(ISession session)
        {
            var freeroamCustomizationData = platform.GetFreeroamCharacterCustomizationData();
            var data = serialization.SerializeAsDesignatedJson(freeroamCustomizationData);
            session?.Client?.TriggerClientEvent(ClientEvents.DisplayCharacterCustomization, data);
            session.State = SessionState.CharacterCustomization;
            return true;
        }
    }
}
