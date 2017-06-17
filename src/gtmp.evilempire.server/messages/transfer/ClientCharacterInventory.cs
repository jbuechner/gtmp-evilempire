using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    class ClientCharacterInventory
    {
        CharacterInventory characterInventory;

        public int CharacterId => characterInventory.CharacterId;

        public IEnumerable<ClientItem> Items { get; private set; }

        public ClientCharacterInventory(CharacterInventory characterInventory)
        {
            if (characterInventory == null)
            {
                throw new ArgumentNullException(nameof(characterInventory));
            }
            this.characterInventory = characterInventory;
            Items = characterInventory.Items.Select(s => new ClientItem(s)).ToList();
        }
    }
}
