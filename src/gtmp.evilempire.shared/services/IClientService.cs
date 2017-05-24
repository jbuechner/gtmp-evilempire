using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IClientService
    {
        IClient CreateFromPlatformObject(object platformObject);

        void RegisterTuple(IClient client, object platformObject);

        IClient FindByPlatformObject(object platformObject);
    }
}
