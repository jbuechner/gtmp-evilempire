using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class ItemDescription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }


        public bool IsStackable { get; set; }
        public int MaximumStack { get; set; }


        public Currency AssociatedCurrency { get; set; }
        public double Denomination { get; set; }
    }
}
