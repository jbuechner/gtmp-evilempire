using gtmp.evilempire.server.actions;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace gtmp.evilempire.server
{
    class ServerTimerRealm
    {
        readonly ServiceContainer services;
        readonly ISessionService sessions;
        readonly ICharacterService characters;
        readonly IDictionary<MapTimer, long> mapTimerInterval = new Dictionary<MapTimer, long>();

        public ServerTimerRealm(ServiceContainer services)
        {
            this.services = services;
            sessions = services.Get<ISessionService>();
            characters = services.Get<ICharacterService>();
            var map = services.Get<Map>();

            foreach (var timer in map.Metadata.Timers)
            {
                mapTimerInterval.Add(timer, 0);
            }
        }

        public void Process(CancellationToken cancellationToken, long delta)
        {
            var keys = mapTimerInterval.Keys.ToArray();
            foreach (var key in keys)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var totalMs = (long)key.Interval.TotalMilliseconds;
                mapTimerInterval[key] += delta;
                if (mapTimerInterval[key] >= totalMs)
                {
                    sessions.ForEachSession(
                        session =>
                        {
                            foreach(var action in key.Actions)
                            {
                                var executionEngine = new ActionExecutionEngine(services, action);
                                executionEngine.Run(session);
                            }
                            return !cancellationToken.IsCancellationRequested;
                        }
                    );
                    mapTimerInterval[key] -= totalMs;
                }
            }
        }
    }
}
