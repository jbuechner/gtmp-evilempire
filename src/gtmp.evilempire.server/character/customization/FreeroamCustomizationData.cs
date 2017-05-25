using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.character.customization
{
    public class FreeroamCustomizationData
    {
        public IList<FreeroamModel> Models { get; } = new List<FreeroamModel>();
        public IList<FreeroamFace> Faces { get; } = new List<FreeroamFace>();
    }
}
