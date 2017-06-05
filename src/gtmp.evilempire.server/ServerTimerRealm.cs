using gtmp.evilempire.entities;
using gtmp.evilempire.server.actions;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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


                            //if (session != null && session.CharacterInventory != null)
                            //{
                            //    // simple approach, we do not store individual timers on the respective characters, therefore a player can login and receive a allowance because the global tick
                            //    // happened not because he was long enough online
                            //    characters.AddToCharacterInventory(session.CharacterInventory.CharacterId, items);
                            //    if (currencies != null && currencies.Length > 0)
                            //    {
                            //        sessions.SendMoneyChangedEvents(session, currencies);
                            //    }
                            //}
                            return !cancellationToken.IsCancellationRequested;
                        }
                    );
                    mapTimerInterval[key] -= totalMs;
                }
            }
        }
    }
}
