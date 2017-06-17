using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public class CharacterInventoryChanges
    {
        public static readonly CharacterInventoryChanges None = new CharacterInventoryChanges(false, null, null, null);

        public bool Any { get; }
        public IEnumerable<Item> AddedOrChangedItems {get;}
        public IEnumerable<Item> RemovedItems { get; }

        public IEnumerable<Currency> ChangedCurrencies { get; }

        public CharacterInventoryChanges(bool any, IEnumerable<Item> addedOrChangedItems, IEnumerable<Item> removedItems, IEnumerable<Currency> changedCurrencies)
        {
            AddedOrChangedItems = addedOrChangedItems ?? new Item[0];
            RemovedItems = removedItems ?? new Item[0];
            ChangedCurrencies = changedCurrencies ?? new Currency[0];
            Any = any;
        }

    }
}
