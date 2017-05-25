using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.character.customization
{
    public class FreeroamModel
    {
        public Gender Gender { get; }
        public long Hash { get; }
        public string Name { get; }

        public FreeroamModel(Gender gender, long hash, string name)
        {
            Gender = gender;
            Hash = hash;
            Name = name;
        }
    }
}
