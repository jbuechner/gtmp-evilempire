using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IServiceResult
    {
        ServiceResultState State { get; }
        object Data { get; }
        Exception Exception { get; }
    }

    public interface IServiceResult<T> : IServiceResult
    {
        new T Data { get; }
    }
}
