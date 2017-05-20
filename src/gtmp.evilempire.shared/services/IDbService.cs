using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface IDbService
    {
        T Select<T, TKey>(TKey key);
    }
}
