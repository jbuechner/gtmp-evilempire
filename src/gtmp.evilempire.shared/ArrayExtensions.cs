using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public static class ArrayExtensions
    {
        public static T At<T>(this T[] array, int index)
        {
            if (array != null && index >= 0 && index < array.Length)
            {
                return array[index];
            }
            return default(T);
        }
    }
}
