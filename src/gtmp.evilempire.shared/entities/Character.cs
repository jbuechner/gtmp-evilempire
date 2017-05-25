using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class Character
    {
        public int Id { get; set; }
        public string AssociatedLogin { get; set; }

        public Vector3f? Position { get; set; }
        public Vector3f? Rotation { get; set; }
    }
}
