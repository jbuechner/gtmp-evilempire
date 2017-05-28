using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server.sessions
{
    class SessionFreeroamStateHandler : SessionStateHandlerBase
    {
        Map map;

        public override SessionState HandledSessionState
        {
            get
            {
                return SessionState.Freeroam;
            }
        }

        public SessionFreeroamStateHandler(ServiceContainer services)
            : base(services)
        {
            map = services.Get<Map>();
        }

        public override bool Transit(ISession session)
        {
            var client = session.Client;
            if (client != null)
            {
                client.Dimension = 0;
                if (session.Character != null)
                {
                    UpdateClientPositionWithLastKnownPosition(map, session);
                }
                client.CanMove = true;
            }

            session?.Client?.TriggerClientEvent(ClientEvents.EnterFreeroam);
            return true;
        }

        static void UpdateClientPositionWithLastKnownPosition(Map map, ISession session)
        {
            var client = session.Client;
            var character = session.Character;

            if (character.Position.HasValue)
            {
                var vector = character.Position.Value;
                vector.Z += 0.2f;
                client.Position = vector;
            }
            else
            {
                var startingPoint = map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0);
                if (startingPoint != null)
                {
                    client.Position = startingPoint.Position;
                }
            }

            if (character.Rotation.HasValue)
            {
                client.Rotation = character.Rotation.Value;
            }
        }
    }
}
