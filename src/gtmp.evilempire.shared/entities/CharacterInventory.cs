using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class CharacterInventory
    {
        public int CharacterId { get; set; }

        public List<Item> Money { get; set; } = new List<Item>();
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
