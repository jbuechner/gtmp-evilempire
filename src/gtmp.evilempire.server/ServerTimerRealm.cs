using gtmp.evilempire.entities;
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
        readonly ISessionService sessions;
        readonly ICharacterService characters;
        readonly IDictionary<MapTimer, long> mapTimerInterval = new Dictionary<MapTimer, long>();
        readonly IDictionary<MapTimer, Currency[]> mapTimerCurrencies = new Dictionary<MapTimer, Currency[]>();

        public ServerTimerRealm(ServiceContainer services)
        {
            sessions = services.Get<ISessionService>();
            characters = services.Get<ICharacterService>();
            var map = services.Get<Map>();

            foreach (var timer in map.Metadata.Timers)
            {
                mapTimerInterval.Add(timer, 0);
                var currencies = timer.Items.Select(s =>
                {
                    ItemDescription itemDescription;
                    if (map.ItemDescriptionMap.TryGetValue(s.ItemDescriptionId, out itemDescription) && itemDescription != null)
                    {
                        return itemDescription.AssociatedCurrency;
                    }
                    return Currency.None;
                }).Where(p => p != Currency.None).ToArray();
                mapTimerCurrencies[timer] = currencies;
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
                    var items = key.Items;
                    var currencies = mapTimerCurrencies[key];
                    sessions.ForEachSession(
                        session =>
                        {
                            if (session != null && session.CharacterInventory != null)
                            {
                                // simple approach, we do not store individual timers on the respective characters, therefore a player can login and receive a allowance because the global tick
                                // happened not because he was long enough online
                                characters.AddToCharacterInventory(session.CharacterInventory.CharacterId, items);
                                if (currencies != null && currencies.Length > 0)
                                {
                                    sessions.SendMoneyChangedEvents(session, currencies);
                                }
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
