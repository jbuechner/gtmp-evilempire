using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    class ClientItemDescription
    {
        ItemDescription itemDescription;

        public int Id => itemDescription.Id;
        public string Name => itemDescription.Name;
        public double Weight => itemDescription.Weight;
        public double Volume => itemDescription.Volume;


        public bool IsStackable => itemDescription.IsStackable;
        public int MaximumStack => itemDescription.MaximumStack;


        public Currency AssociatedCurrency => itemDescription.AssociatedCurrency;
        public double Denomination => itemDescription.Denomination;

        public string Description => itemDescription.Description;

        public ClientItemDescription(ItemDescription itemDescription)
        {
            if (itemDescription == null)
            {
                throw new ArgumentNullException(nameof(itemDescription));
            }
            this.itemDescription = itemDescription;
        }
    }
}
