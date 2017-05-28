using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    public class ClientCharacter
    {
        Character character;

        public bool HasBeenThroughInitialCustomization => character.HasBeenThroughInitialCustomization;
        public Gender Gender => character.Gender;

        public ClientCharacter(Character character)
        {
            this.character = character;
        }
    }
}
