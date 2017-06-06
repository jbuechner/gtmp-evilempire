using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.actions
{
    abstract class ActionConditionalComparator
    {
        class EqualsComparator : ActionConditionalComparator
        {
            public override bool Compare(object a, object b)
            {
                if (a == null && b == null)
                {
                    return true;
                }
                if (a != null && b != null)
                {
                    return a.Equals(b);
                }
                return false;
            }
        }

        static IDictionary<string, ActionConditionalComparator> comparatorMap = CreateComparatorMap();

        ActionConditionalComparator()
        {
        }

        public abstract bool Compare(object a, object b);

        static IDictionary<string, ActionConditionalComparator> CreateComparatorMap()
        {
            var result = new Dictionary<string, ActionConditionalComparator>
            {
                { "Equals", new EqualsComparator() }
            };
            return result;
        }

        public static ActionConditionalComparator GetComparator(string comparator)
        {
            ActionConditionalComparator implementation;
            if (comparatorMap.TryGetValue(comparator, out implementation))
            {
                return implementation;
            }
            return null;
        }
    }
}
