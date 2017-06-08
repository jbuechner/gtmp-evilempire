﻿using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    class ClientItem
    {
        Item item;

        public string Id => item.Id.ToString(CultureInfo.InvariantCulture);
        public int ItemDescriptionId => item.ItemDescriptionId;
        public int Amount => item.Amount;

        public ClientItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            this.item = item;
        }
    }
}
