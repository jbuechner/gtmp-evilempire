using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IItemService
    {
        IEnumerable<Item> CreateMoney(Currency currency, int amount);
        IEnumerable<Item> CreateItem(ItemDescription itemDescription, int amount, string name, long? keyForEntityId);
        IEnumerable<Item> CreateItem(int itemDescriptionId, int amount, string name, long? keyForEntityId);
        ItemDescription GetItemDescription(int itemDescriptionId);

        IEnumerable<ItemDescription> GetItemDescriptions();
    }
}
