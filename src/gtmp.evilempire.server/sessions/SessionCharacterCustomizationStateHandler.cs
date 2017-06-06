using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server.sessions
{
    class SessionCharacterCustomizationStateHandler : SessionStateHandlerBase
    {
        Map map;
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
            map = services.Get<Map>();
            platform = services.Get<IPlatformService>();
            serialization = services.Get<ISerializationService>();
        }

        public override bool Transit(ISession session)
        {
            var freeroamCustomizationData = platform.GetFreeroamCharacterCustomizationData();
            var data = serialization.SerializeAsDesignatedJson(freeroamCustomizationData);
            session?.Client?.TriggerClientEvent(ClientEvents.DisplayCharacterCustomization, data);
            session.State = SessionState.CharacterCustomization;
            session.UpdateDatabasePosition = false;
            session.Client.Dimension = session.PrivateDimension;

            var mapPoint = map.GetPointByName(WellKnownNamedPoints.CharacterCustomizationLocation);
            if (mapPoint != null)
            {
                session.Client.Position = mapPoint.Position;
                if (mapPoint.Rotation.HasValue)
                {
                    session.Client.Rotation = mapPoint.Rotation.Value;
                }
            }

            return true;
        }
    }
}
