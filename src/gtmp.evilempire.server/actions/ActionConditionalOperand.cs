using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace gtmp.evilempire.server.actions
{
    abstract class ActionConditionalOperand
    {
        public class Constant : ActionConditionalOperand
        {
            Type resultType;
            object value;

            public override Type ResultType
            {
                get
                {
                    return resultType;
                }
            }

            public Constant(object value)
            {
                this.value = value;
                resultType = value == null ? typeof(object) : value.GetType();
            }

            public override object ProvideValue(ActionExecutionContext context)
            {
                return value;
            }
        }

        public class Property : ActionConditionalOperand
        {
            string propertyName;
            Type resultType;
            Func<ActionExecutionContext, object> getter;

            public override Type ResultType
            {
                get
                {
                    return resultType;
                }
            }

            public Property(string propertyName)
            {
                this.propertyName = propertyName;


                var parts = this.propertyName.Split('.');
                if (parts != null)
                {
                    var inputParameter = Expression.Parameter(typeof(ActionExecutionContext));

                    var current = inputParameter.Type;
                    ParameterExpression input = inputParameter;
                    var nullConstant = Expression.Constant(null);
                    var variables = new List<ParameterExpression>();
                    var expressions = new List<Expression>();
                    var returnLabel = Expression.Label(typeof(object));

                    foreach (var part in parts)
                    {
                        var member = current.GetMember(part);
                        if (member == null)
                        {
                            using (ConsoleColor.Yellow.Foreground())
                            {
                                Console.WriteLine($"[ActionConditionalOperand] There is no property called \"{part}\" on type {current.Name}.");
                                throw new ArgumentException(nameof(propertyName));
                            }
                        }

                        var equals = Expression.Equal(input, nullConstant);
                        var nullCheck = Expression.IfThen(equals, Expression.Return(returnLabel, nullConstant));

                        expressions.Add(nullCheck);

                        var accessor = Expression.PropertyOrField(input, part);
                        current = accessor.Type;
                        input = Expression.Variable(accessor.Type);
                        variables.Add(input);
                        var assignment = Expression.Assign(input, accessor);
                        expressions.Add(assignment);
                    }

                    var resultVariable = variables.Last();
                    resultType = resultVariable.Type;

                    var conversion = Expression.Convert(resultVariable, typeof(object));
                    expressions.Add(Expression.Return(returnLabel, conversion));
                    expressions.Add(conversion);
                    expressions.Add(Expression.Label(returnLabel, nullConstant));

                    var body = Expression.Block(variables, expressions);
                    var lambda = Expression.Lambda<Func<ActionExecutionContext, object>>(body, inputParameter);
                    getter = lambda.Compile();
                }
            }

            public override object ProvideValue(ActionExecutionContext context)
            {
                return getter(context);
            }
        }

        public abstract Type ResultType { get; }

        public abstract object ProvideValue(ActionExecutionContext context);
    }
}
