using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities.customization
{
    public class FreeroamHairColor
    {
        public Gender Gender { get; }
        public int Id { get; }

        public FreeroamHairColor(int id)
        {
            Id = id;
        }
    }
}
