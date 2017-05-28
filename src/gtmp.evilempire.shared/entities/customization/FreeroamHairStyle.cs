using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities.customization
{
    public class FreeroamHairStyle
    {
        public Gender Gender { get; }
        public int Id { get; }

        public bool AvailableDuringCharacterCustomization { get; set; }

        public FreeroamHairStyle(Gender gender, int id)
        {
            Gender = gender;
            Id = id;
        }
    }
}
