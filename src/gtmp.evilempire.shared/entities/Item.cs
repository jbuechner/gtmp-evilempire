using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class Item
    {
        public static readonly long ZeroId = long.MinValue;

        public long Id { get; set; } = ZeroId;
        public int ItemDescriptionId { get; set; }

        public int Amount { get; set; }

        public string Name { get; set; }

        public long? KeyForEntityId { get; set; }

        public Item()
        {
        }

        public Item(Item other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ItemDescriptionId = other.ItemDescriptionId;
            Amount = other.Amount;
            Name = other.Name;
            KeyForEntityId = KeyForEntityId;
        }
    }
}
