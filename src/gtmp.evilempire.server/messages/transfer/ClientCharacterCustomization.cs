using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    class ClientCharacterCustomization
    {
        CharacterCustomization characterCustomization;

        public int ModelHash
        {
            get
            {
                return characterCustomization.ModelHash;
            }
        }

        public ClientCharacterCustomization(CharacterCustomization characterCustomization)
        {
            this.characterCustomization = characterCustomization;
        }
    }
}
