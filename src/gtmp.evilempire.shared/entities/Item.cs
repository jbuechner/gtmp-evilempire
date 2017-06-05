using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class Item
    {
        public int Id { get; set; }
        public int ItemDescriptionId { get; set; }

        public int Amount { get; set; }
    }
}
