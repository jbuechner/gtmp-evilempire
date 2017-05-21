using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public interface IClient : IEquatable<IClient>
    {
        object PlatformObject { get; }
    }
}
