using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.sessions
{
    class SessionConnectedStateHandler : SessionStateHandlerBase
    {
        Map map;

        public override SessionState HandledSessionState
        {
            get
            {
                return SessionState.Connected;
            }
        }

        public SessionConnectedStateHandler(ServiceContainer services)
            : base(services)
        {
            map = services.Get<Map>();
        }

        public override bool Transit(ISession session)
        {
            var loadingPoint = map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0)?.Position ?? Vector3f.One;
            var client = session.Client;
            client.IsNametagVisible = false;
            client.Dimension = session.PrivateDimension;
            client.CanMove = false;
            client.Position = loadingPoint;
            client.StopAnimation();
            session.State = SessionState.Connected;
            return true;
        }
    }
}
