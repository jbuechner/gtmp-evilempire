using gtmp.evilempire.entities;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace gtmp.evilempire.server.services
{
    class CharacterService : ICharacterService
    {
        Map map;
        IDbService db;
        IItemService items;
        IPlatformService platform;

        ConcurrentDictionary<int, ConcurrentDictionary<Currency, double>> characterMoney = new ConcurrentDictionary<int, ConcurrentDictionary<Currency, double>>();
        ConcurrentDictionary<int, object> characterInventoriesInModification = new ConcurrentDictionary<int, object>();

        public CharacterService(Map map, IDbService db, IPlatformService platform, IItemService items)
        {
            this.map = map;
            this.db = db;
            this.platform = platform;
            this.items = items;
        }

        public Character GetActiveCharacter(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var character = db.SelectMany<Character, string>(session.User.Login)?.FirstOrDefault();
            if (character == null)
            {
                var characterId = db.NextValueFor(Constants.Database.Sequences.CharacterIdSequence);
                character = new Character { AssociatedLogin = session.User.Login, Id = characterId };
                db.Insert(character);
            }

            return character;
        }

        public void UpdatePosition(int characterId, Vector3f? position, Vector3f? rotation)
        {
            var character = GetCharacterById(characterId);
            if (position.HasValue)
            {
                character.Position = position;
            }
            if (rotation.HasValue)
            {
                character.Rotation = rotation;
            }
            db.Update<Character>(character);
        }

        public Character GetCharacterById(int characterId)
        {
            var character = db.Select<Character, int>(ks => ks.Id, characterId);
            return character;
        }

        public CharacterCustomization GetCharacterCustomizationById(int characterId)
        {
            var characterCustomization = db.Select<CharacterCustomization, int>(characterId);
            return characterCustomization;
        }

        public CharacterInventory GetCharacterInventoryById(int characterId)
        {
            var characterInventory = db.Select<CharacterInventory, int>(characterId);
            if (characterInventory != null)
            {
                UpdateCharacterMoneyStatistic(characterInventory);
            }
            return characterInventory;
        }

        public Item GetCharacterItemById(int characterId, long itemId)
        {
            var characterInventory = db.Select<CharacterInventory, int>(characterId);
            if (characterInventory != null)
            {
                var item = characterInventory.Items.FirstOrDefault(p => p.Id == itemId);
                if (item != null)
                {
                    return item;
                }
                item = characterInventory.Money.FirstOrDefault(p => p.Id == itemId);
                return item;
            }
            return null;
        }

        public CharacterCustomization CreateDefaultCharacterCustomization(int characterId)
        {
            var characterCustomization = platform.GetDefaultCharacterCustomization();
            characterCustomization.CharacterId = characterId;
            characterCustomization.Gender = Gender.Male;
            db.Insert<CharacterCustomization>(characterCustomization);
            return characterCustomization;
        }

        public CharacterInventory CreateDefaultCharacterInventory(int characterId)
        {
            var characterInventory = new CharacterInventory() { CharacterId = characterId };
            db.Insert<CharacterInventory>(characterInventory);
            AddToCharacterInventory(characterInventory, map.Metadata.StartingInventoryItems);
            return characterInventory;
        }

        public IEnumerable<Item> GetKeys(int characterId, Vehicle vehicle)
        {
            var characterInventory = GetCharacterInventoryById(characterId);
            if (characterInventory == null)
            {
                yield break;
            }
            foreach(var item in characterInventory.Items)
            {
                if (item == null)
                {
                    continue;
                }
                if (item.KeyForEntityId.HasValue && item.KeyForEntityId.Value == vehicle.Id)
                {
                    yield return item;
                }
            }
        }

        public double GetTotalAmountOfMoney(int characterId, Currency currency)
        {
            ConcurrentDictionary<Currency, double> money;
            if (characterMoney.TryGetValue(characterId, out money) && money != null)
            {
                double value;
                if (money.TryGetValue(currency, out value))
                {
                    return value;
                }
            }
            return 0;
        }

        public void AddToCharacterInventory(int characterId, IEnumerable<Item> items)
        {
            var characterInventory = GetCharacterInventoryById(characterId);
            if (characterInventory != null)
            {
                AddToCharacterInventory(characterInventory, items);
            }
        }

        public CharacterInventoryChanges RemoveFromCharacterInventory(int characterId, IEnumerable<Item> items)
        {
            var characterInventory = GetCharacterInventoryById(characterId);
            if (characterInventory != null)
            {
                return RemoveFromCharacterInventory(characterInventory, items);
            }
            return CharacterInventoryChanges.None;
        }

        void AddToCharacterInventory(CharacterInventory characterInventory, IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                var newItems = this.items.CreateItem(item.ItemDescriptionId, item.Amount, item.Name, item.KeyForEntityId);
                AddItemsToCharacterInventory(characterInventory, newItems);
            }
        }

        void AddItemsToCharacterInventory(CharacterInventory characterInventory, IEnumerable<Item> items)
        {
            if (characterInventory == null)
            {
                throw new ArgumentNullException(nameof(characterInventory));
            }

            var sync = characterInventoriesInModification.GetOrAdd(characterInventory.CharacterId, _ => new object());
            var hasMoneyChanged = false;
            lock (sync)
            {
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var itemDescription = this.items.GetItemDescription(item.ItemDescriptionId);
                        if (itemDescription == null)
                        {
                            using (ConsoleColor.Yellow.Foreground())
                            {
                                Console.WriteLine($"[AddToCharacterInventory] The item {item.Id} uses an unknwon item description id {item.ItemDescriptionId} to add an item to character inventory with id {characterInventory.CharacterId}. Skipped.");
                            }
                            continue;
                        }

                        IList<Item> targetList = null;
                        if (itemDescription.AssociatedCurrency == Currency.None)
                        {
                            targetList = characterInventory.Items;
                        }
                        else
                        {
                            targetList = characterInventory.Money;
                            hasMoneyChanged = true;
                        }

                        // Stack amount to existing items with stack size left
                        var itemsWithAvailableAmountLeft = targetList.Where(p => p.ItemDescriptionId == item.ItemDescriptionId && p.Amount < itemDescription.MaximumStack);
                        var stackLeft = item.Amount;
                        foreach(var itemWithAvailableAmountLeft in itemsWithAvailableAmountLeft)
                        {
                            var available = itemDescription.MaximumStack - itemWithAvailableAmountLeft.Amount;
                            var used = available > stackLeft ? stackLeft : available;
                            stackLeft -= used;
                            item.Amount -= used;
                            itemWithAvailableAmountLeft.Amount += used;

                            if (stackLeft <= 0)
                            {
                                break;
                            }
                        }

                        if (item.Amount > 0)
                        {
                            item.Id = item.Id == Item.ZeroId ? db.NextInt64ValueFor(Constants.Database.Sequences.ItemIdSequence) : item.Id;
                            targetList.Add(item);
                        }
                    }
                }
                db.Update<CharacterInventory>(characterInventory);
            }
            if (hasMoneyChanged)
            {
                UpdateCharacterMoneyStatistic(characterInventory);
            }
        }

        public CharacterInventoryChanges RemoveFromCharacterInventory(CharacterInventory characterInventory, IEnumerable<Item> items)
        {
            KeyValuePair<int?, IList<Item>> selectNextCandidate(CharacterInventory inv, Item item, ItemDescription itemDescription, out Item candidate) {
                var index = inv.Items.IndexOf(p => p != null && p.Id == item.Id, out candidate);
                if (index != null)
                {
                    return new KeyValuePair<int?, IList<Item>>(index, inv.Items);
                }
                index = inv.Money.IndexOf(p => p != null && p.Id == item.Id, out candidate);
                if (index != null)
                {
                    return new KeyValuePair<int?, IList<Item>>(index, inv.Money);
                }
                if (itemDescription != null && itemDescription.IsStackable)
                {
                    index = inv.Items.IndexOf(p => p != null && p.ItemDescriptionId == itemDescription.Id, out candidate);
                    if (index != null)
                    {
                        return new KeyValuePair<int?, IList<Item>>(index, inv.Items);
                    }
                    inv.Money.IndexOf(p => p != null && p.ItemDescriptionId == itemDescription.Id, out candidate);
                    if (index != null)
                    {
                        return new KeyValuePair<int?, IList<Item>>(index, inv.Money);
                    }
                }
                return new KeyValuePair<int?, IList<Item>>(null, null);
            }

            characterInventory = characterInventory ?? throw new ArgumentNullException(nameof(characterInventory));

            if (characterInventory == null)
            {
                throw new ArgumentNullException(nameof(characterInventory));
            }
            if (items == null)
            {
                return CharacterInventoryChanges.None;
            }

            var changedItems = new Dictionary<long, Item>();
            var removedItems = new Dictionary<long, Item>();
            var changedCurrencies = new HashSet<Currency>();
            var sync = characterInventoriesInModification.GetOrAdd(characterInventory.CharacterId, _ => new object());
            var hasMoneyChanged = false;
            lock (sync)
            {
                foreach (var item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    var itemDescription = this.items.GetItemDescription(item.ItemDescriptionId);
                    var remaining = item.Amount;

                    Item nextCandidate = null;
                    KeyValuePair<int?, IList<Item>> nextCandidateKeyValue = new KeyValuePair<int?, IList<Item>>(null, null);
                    do
                    {
                        if (remaining > 0)
                        {
                            nextCandidateKeyValue = selectNextCandidate(characterInventory, item, itemDescription, out nextCandidate);
                            if (nextCandidate == null)
                            {
                                return CharacterInventoryChanges.None;
                            }
                        }

                        if (itemDescription == null)
                        {
                            itemDescription = this.items.GetItemDescription(nextCandidate.ItemDescriptionId);
                        }

                        if (itemDescription.AssociatedCurrency != Currency.None)
                        {
                            changedCurrencies.Add(itemDescription.AssociatedCurrency);
                        }

                        if (nextCandidate.Amount > remaining)
                        {

                            nextCandidate.Amount -= remaining;
                            remaining = 0;
                            changedItems[nextCandidate.Id] = nextCandidate;
                        }
                        else
                        {
                            remaining -= nextCandidate.Amount;
                            nextCandidate.Amount = 0;
                            nextCandidateKeyValue.Value.RemoveAt(nextCandidateKeyValue.Key.Value);
                            if (changedItems.ContainsKey(nextCandidate.Id))
                            {
                                changedItems.Remove(nextCandidate.Id);
                            }
                            removedItems[nextCandidate.Id] = nextCandidate;
                        }
                    } while (remaining > 0 && nextCandidate != null);

                    if (remaining > 0)
                    {
                        return CharacterInventoryChanges.None;
                    }
                }
                db.Update<CharacterInventory>(characterInventory);
            }
            if (hasMoneyChanged)
            {
                UpdateCharacterMoneyStatistic(characterInventory);
            }
            return new CharacterInventoryChanges(true, changedItems.Values, removedItems.Values, changedCurrencies);
        }

        void UpdateCharacterMoneyStatistic(CharacterInventory characterInventory)
        {
            if (characterInventory == null)
            {
                throw new ArgumentNullException(nameof(characterInventory));
            }

            var characterMoneyStatistic = characterMoney.GetOrAdd(characterInventory.CharacterId, key => new ConcurrentDictionary<Currency, double>());
            characterMoneyStatistic.Clear();

            var money = characterInventory.Money;
            if (money != null)
            {
                var index = -1;
                while (++index < money.Count)
                {
                    try
                    {
                        var item = money[index];
                        if (item != null)
                        {
                            var itemDescription = items.GetItemDescription(item.ItemDescriptionId);
                            if (itemDescription == null)
                            {
                                using (ConsoleColor.Yellow.Foreground())
                                {
                                    Console.WriteLine($"[UpdateCharacterMoneyStatistic] WARNING for character id {characterInventory.CharacterId}. The character has a item with id {item.Id} of item description {item.ItemDescriptionId} where the id does not points to a known item description.");
                                }
                                continue;
                            }
                            if (itemDescription.AssociatedCurrency == Currency.None)
                            {
                                using (ConsoleColor.Yellow.Foreground())
                                {
                                    Console.WriteLine($"[UpdateCharacterMoneyStatistic] WARNING for character id {characterInventory.CharacterId}. The character has a item with id {item.Id} of item description {item.ItemDescriptionId} that has no associated currency.");
                                }
                                continue;
                            }

                            var itemValue = item.Amount * itemDescription.Denomination;
                            characterMoneyStatistic.AddOrUpdate(itemDescription.AssociatedCurrency, itemValue, (key, value) => value + itemValue);
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }
                }
            }
        }
    }
}
