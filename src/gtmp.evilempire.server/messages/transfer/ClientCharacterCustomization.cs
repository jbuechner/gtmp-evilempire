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

        public int ModelHash => characterCustomization.ModelHash;

        public int HairStyleId => characterCustomization.HairStyleId;

        public int HairColorId => characterCustomization.HairColorId;

        public Gender Gender => characterCustomization.Gender;

        public ClientCharacterFaceCustomization Face { get; }

        public ClientCharacterCustomization(CharacterCustomization characterCustomization)
        {
            Face = new ClientCharacterFaceCustomization(characterCustomization.Face);
            this.characterCustomization = characterCustomization;
        }
    }
}
