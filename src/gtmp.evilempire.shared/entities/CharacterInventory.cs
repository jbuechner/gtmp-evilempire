using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    //todo: Implement In-Memory View of CharacterInventory, especially CharacterService associated with Sessions, to allow optimized MemoryAccess because storage seperation of ordinary Items and Money representations (also items) does not fulfil complete requirements of O(1) access and makes storage and access unnecessary complex
    public class CharacterInventory
    {
        public int CharacterId { get; set; }
        
        public List<Item> Money { get; set; } = new List<Item>();
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
