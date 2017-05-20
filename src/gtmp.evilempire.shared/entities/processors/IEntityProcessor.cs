using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities.processors
{
    public interface IEntityProcessor
    {
        void Process(object entity);
    }
}
