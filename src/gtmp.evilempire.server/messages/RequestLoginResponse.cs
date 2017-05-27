using gtmp.evilempire.server.messages.transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages
{
    class RequestLoginResponse
    {
        public ClientUser User { get; set; }
        public ClientCharacter Character { get; set; }
        public ClientCharacterCustomization CharacterCustomization { get; set; }
    }
}
