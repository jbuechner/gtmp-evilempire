﻿using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.entities;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server.services
{
    class ItemService : IItemService
    {
        IDbService db;
        IDictionary<int, ItemDescription> ItemDescriptions { get; } = new Dictionary<int, ItemDescription>();

        IDictionary<Tuple<Currency, double>, ItemDescription> CurrencyItems { get; } = new Dictionary<Tuple<Currency, double>, ItemDescription>();

        public ItemService(Map map, IDbService db)
        {
            InitializeFromMapItemDescriptions(map.ItemDescriptionMap.Values);

            this.db = db;
        }

        public IEnumerable<Item> CreateMoney(Currency currency, int amount)
        {
            const double denomination = 1d;
            ItemDescription itemDescription;
            if (!CurrencyItems.TryGetValue(new Tuple<Currency, double>(currency, denomination), out itemDescription))
            {
                using (ConsoleColor.Red.Foreground())
                {
                    Console.WriteLine($"There is no item description for currency {currency} and denomination {denomination}.");
                    return null;
                }
            }

            return CreateItem(itemDescription, amount);
        }

        public IEnumerable<Item> CreateItem(int itemDescriptionId, int amount)
        {
            ItemDescription itemDescription = GetItemDescription(itemDescriptionId);
            if (itemDescription == null)
            {
                using (ConsoleColor.Red.Foreground())
                {
                    Console.WriteLine($"There is no item description for id {itemDescriptionId}.");
                    return null;
                }
            }
            return CreateItem(itemDescription, amount);
        }

        public IEnumerable<Item> CreateItem(ItemDescription itemDescription, int amount)
        {
            if (itemDescription == null)
            {
                throw new ArgumentNullException(nameof(itemDescription));
            }

            while (amount > 0)
            {
                var nextStack = amount > itemDescription.MaximumStack ? itemDescription.MaximumStack : amount;
                var newItem = CreateSingleItem(itemDescription.Id, nextStack);
                yield return newItem;
                amount -= nextStack;
            }
        }

        public ItemDescription GetItemDescription(int itemDescriptionId)
        {
            ItemDescription itemDescription;
            if (ItemDescriptions.TryGetValue(itemDescriptionId, out itemDescription))
            {
                return itemDescription;
            }
            return null;
        }

        Item CreateSingleItem(int itemDescriptionId, int amount)
        {
            var itemId = db.NextInt64ValueFor(Constants.Database.Sequences.ItemIdSequence);
            var item = new Item { Id = itemId, ItemDescriptionId = itemDescriptionId, Amount = amount };
            return item;
        }

        void InitializeFromMapItemDescriptions(IEnumerable<ItemDescription> itemDescriptions)
        {
            if (itemDescriptions == null)
            {
                return;
            }
            foreach(var itemDescription in itemDescriptions)
            {
                if (itemDescription == null)
                {
                    continue;
                }
                ItemDescriptions[itemDescription.Id] = itemDescription;

                if (itemDescription.AssociatedCurrency != Currency.None)
                {
                    var tuple = new Tuple<Currency, double>(itemDescription.AssociatedCurrency, itemDescription.Denomination);
                    if (CurrencyItems.ContainsKey(tuple))
                    {
                        using (ConsoleColor.Yellow.Foreground())
                        {
                            Console.WriteLine($"The item descripion {itemDescription.Id} Currency = {tuple.Item1}, Denomination = {tuple.Item2} is a duplicate of an existing item description with id {CurrencyItems[tuple].Id}. Skipping.");
                        }
                        continue;
                    }
                    CurrencyItems[tuple] = itemDescription;
                }
            }
        }
    }
}
