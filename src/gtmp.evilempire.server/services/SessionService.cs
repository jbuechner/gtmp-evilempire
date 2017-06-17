using gtmp.evilempire.services;
using System;
using System.Linq;
using gtmp.evilempire.sessions;
using System.Collections.Concurrent;
using gtmp.evilempire.server.sessions;
using System.Collections.Generic;
using gtmp.evilempire.entities;
using gtmp.evilempire.server.messages.transfer;
using System.Globalization;

namespace gtmp.evilempire.server.services
{
    class SessionService : ISessionService
    {
        const int PrivateDimensionsOffset = 1500000;
        const int NumberOfMaximumPrivateDimensions = 10000;

        bool updateSessionsCopy = true;
        bool isSessionsCopyDirty = false;
        IList<ISession> sessionsCopy = null;

        ConcurrentDictionary<Session, byte> sessions = new ConcurrentDictionary<Session, byte>();
        ConcurrentDictionary<IClient, Session> clientToSessionMap = new ConcurrentDictionary<IClient, Session>();
        ConcurrentDictionary<string, Session> loginToSessionMap = new ConcurrentDictionary<string, Session>();

        ISerializationService serialization;
        ISessionStateTransitionService sessionStateTransitionService;
        IItemService items;
        ICharacterService characters;

        ConcurrentDictionary<int, byte> usedPrivateDimensions = new ConcurrentDictionary<int, byte>();

        public SessionService(ISerializationService serialization, ISessionStateTransitionService sessionStateTransitionService, ICharacterService characters, IItemService items)
        {
            this.serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
            this.sessionStateTransitionService = sessionStateTransitionService ?? throw new ArgumentNullException(nameof(sessionStateTransitionService));
            this.characters = characters ?? throw new ArgumentNullException(nameof(characters));
            this.items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public ISession CreateSession(IClient client)
        {
            var session = new Session(client) { PrivateDimension = GetUniquePrivateDimension(), UpdateDatabasePosition = false };
            using (ConsoleColor.Cyan.Foreground())
                Console.WriteLine($"[{client.Name}] Private Dimensions = {session.PrivateDimension} ");
            sessions.TryAdd(session, 1);
            clientToSessionMap.TryAdd(client, session);
            isSessionsCopyDirty = true;
            UpdateSessionsCopy();
            return session;
        }

        public ISession GetSession(IClient client)
        {
            Session session;
            if (clientToSessionMap.TryGetValue(client, out session))
            {
                return session;
            }
            return null;
        }

        public ISession GetSessionByLogin(string login)
        {
            Session session;
            if (loginToSessionMap.TryGetValue(login, out session))
            {
                return session;
            }
            return null;
        }

        public void AssociateSessionWithLogin(ISession session, string login)
        {
            var sessionObject = session as Session;
            if (sessionObject == null)
            {
                throw new NotImplementedException();
            }
            loginToSessionMap.AddOrUpdate(login, sessionObject, (k, v) => sessionObject);
        }

        public void RemoveStaleSessions()
        {
            using (new DelegatedDisposable(() => updateSessionsCopy = true))
            {
                updateSessionsCopy = false;

                if (sessionsCopy != null)
                {
                    foreach (var session in sessionsCopy)
                    {
                        if (!session.Client.IsConnected)
                        {
                            using (ConsoleColor.Yellow.Foreground())
                            {
                                Console.WriteLine($"Removed stale session for client {session.Client.Name}");
                                RemoveSession(session);
                            }
                        }
                    }
                }
            }
            UpdateSessionsCopy();
        }

        public void RemoveSession(ISession session)
        {
            var sessionObject = session as Session;
            if (sessionObject == null)
            {
                throw new NotImplementedException();
            }

            byte v;
            Session v2;
            clientToSessionMap.TryRemove(session.Client, out v2);
            if (session.User != null)
            {
                loginToSessionMap.TryRemove(session.User.Login, out v2);
            }
            sessions.TryRemove(sessionObject, out v);
            FreeDimension(session.PrivateDimension);

            if (session.Client.IsConnected)
            {
                session.Client.Kick("Session removed.");
            }
            isSessionsCopyDirty = true;
            UpdateSessionsCopy();
        }

        public void StoreSessionState()
        {
            if (sessionsCopy != null)
            {
                foreach (var session in sessionsCopy)
                {
                    if (session.Character == null || session.State != SessionState.Freeroam)
                    {
                        continue;
                    }

                    if (session.UpdateDatabasePosition)
                    {
                        var client = session.Client;
                        var position = client.Position;
                        var rotation = client.Rotation;
                        characters.UpdatePosition(session.Character.Id, position, rotation);
                    }
                }
            }
        }

        public void SendCharacterInventoryChangedEvents(ISession session, CharacterInventoryChanges changes)
        {
            session = session ?? throw new ArgumentNullException(nameof(session));
            if (changes == null || !changes.Any)
            {
                return;
            }

            SendItemChangedEvents(session, changes.AddedOrChangedItems, changes.RemovedItems);

            var currencies = changes.ChangedCurrencies.ToArray();
            if (currencies != null && currencies.Length > 0)
            {
                SendMoneyChangedEvents(session, currencies);
            }
        }

        public void SendMoneyChangedEvents(ISession session, params Currency[] currencies)
        {
            session = session ?? throw new ArgumentNullException(nameof(session));
            if (currencies == null || currencies.Length < 1)
            {
                currencies = Enum.GetValues(typeof(Currency)).Cast<Currency>().Where(p => p != Currency.None).ToArray();
            }

            foreach (var currency in currencies)
            {
                var amount = characters.GetTotalAmountOfMoney(session.Character.Id, currency);
                session.Client.TriggerClientEvent(ClientEvents.MoneyChanged, (int)currency, amount);
            }
        }

        public void SendItemChangedEvents(ISession session, IEnumerable<Item> addedOrChangedItems, IEnumerable<Item> removedItems)
        {
            Dictionary<int, ItemDescription> cachedDescriptions = new Dictionary<int, ItemDescription>();
            ItemDescription lookupItemDescription(IDictionary<int, ItemDescription> cache, int itemDescriptionId)
            {
                ItemDescription itemDescription;
                if (cache.TryGetValue(itemDescriptionId, out itemDescription))
                {
                    return itemDescription;
                }
                return cache[itemDescriptionId] = items.GetItemDescription(itemDescriptionId);
            }

            void addToInventory(CharacterInventory inventory, Item item)
            {
                var itemDescription = lookupItemDescription(cachedDescriptions, item.ItemDescriptionId);
                inventory.Items.Add(item);
            }
            void markAsRemoved(ClientCharacterInventory inventory, Item item)
            {
                var clientItem = inventory.Items.FirstOrDefault(p => p.Id == item.Id.ToString(CultureInfo.InvariantCulture));
                if (clientItem != null)
                {
                    clientItem.HasBeenDeleted = true;
                }
            }

            session = session ?? throw new ArgumentNullException(nameof(session));
            if (addedOrChangedItems == null || removedItems == null)
            {
                return;
            }

            var characterId = session.Character?.Id;
            var client = session.Client;
            if (client != null && characterId.HasValue)
            {
                var hasAtLeastOneChange = false;
                CharacterInventory inventory = new CharacterInventory { CharacterId = characterId.Value };
                removedItems = removedItems.ToList(); // multiple iterations
                foreach (var item in addedOrChangedItems.Union(removedItems))
                {
                    addToInventory(inventory, item);
                    hasAtLeastOneChange |= true;
                }

                var response = new ClientCharacterInventory(inventory);
                foreach (var removedItem in removedItems)
                {
                    if (removedItem == null)
                    {
                        continue;
                    }
                    markAsRemoved(response, removedItem);
                    hasAtLeastOneChange |= true;
                }

                if (hasAtLeastOneChange)
                {
                    var data = serialization.SerializeAsDesignatedJson(response);
                    client.TriggerClientEvent(ClientEvents.CharacterInventoryChanged, data);
                }
            }
        }

        public void ForEachSession(Func<ISession, bool> fn)
        {
            if (fn == null || sessionsCopy == null)
            {
                return;
            }

            using (new DelegatedDisposable(() => updateSessionsCopy = true))
            {
                updateSessionsCopy = false;

                if (sessionsCopy != null)
                {
                    foreach (var session in sessionsCopy)
                    {
                        if (!fn(session))
                        {
                            break;
                        }
                    }
                }
            }
            UpdateSessionsCopy();
        }

        void UpdateSessionsCopy()
        {
            if (updateSessionsCopy && isSessionsCopyDirty)
            {
                sessionsCopy = new List<ISession>(sessions.Keys);
                isSessionsCopyDirty = false;
            }
        }

        void FreeDimension(int dimension)
        {
            byte v;
            usedPrivateDimensions.TryRemove(dimension, out v);
        }

        int GetUniquePrivateDimension()
        {
            byte v;
            for (var i = PrivateDimensionsOffset; i < PrivateDimensionsOffset + NumberOfMaximumPrivateDimensions; i++)
            {
                if (!usedPrivateDimensions.TryGetValue(i, out v))
                {
                    if (usedPrivateDimensions.TryAdd(i, 1))
                    {
                        return i;
                    }
                }
            }
            return 0;
        }
    }
}
