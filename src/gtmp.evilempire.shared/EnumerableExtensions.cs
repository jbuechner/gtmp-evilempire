using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public static class EnumerableExtensions
    {
        public static int? IndexOf<T>(this IList<T> list, Func<T, bool> predicate)
        {
            if (list == null || predicate == null)
            {
                return null;
            }
            for (var i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    return i;
                }
            }
            return null;
        }

        public static int? IndexOf<T>(this IList<T> list, Func<T, bool> predicate, out T item)
        {
            if (list == null || predicate == null)
            {
                item = default(T);
                return null;
            }
            for (var i = 0; i < list.Count; i++)
            {
                item = list[i];
                if (predicate(item))
                {
                    return i;
                }
            }
            item = default(T);
            return null;
        }
    }
}
