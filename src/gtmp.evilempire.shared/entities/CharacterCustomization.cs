using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class CharacterCustomization
    {
        public int CharacterId { get; set; }

        public int ModelHash { get; set; }

        public CharacterFaceCustomization Face { get; set; } = new CharacterFaceCustomization();
    }
}
