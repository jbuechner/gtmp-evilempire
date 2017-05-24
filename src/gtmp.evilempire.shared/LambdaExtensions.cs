using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public static class LambdaExtensions
    {
        public static string MemberName<T, TSel>(this Expression<Func<T, TSel>> expression)
        {
            if (expression == null)
            {
                return null;
            }
            if (expression.Body is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression.Body;
                return memberExpression.Member.Name;
            }
            return null;
        }
    }
}
