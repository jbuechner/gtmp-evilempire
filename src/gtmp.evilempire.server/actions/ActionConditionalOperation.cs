using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using System.Linq.Expressions;

namespace gtmp.evilempire.server.actions
{
    class ActionConditionalOperation : ActionConditionalOperand
    {
        public static readonly ActionConditionalOperation True = new ActionConditionalOperation();

        Func<object, object> operandBConversion = null;

        public ActionConditionalComparator Comparator { get; }
        public ActionConditionalOperand OperandA { get; }
        public ActionConditionalOperand OperandB { get; }

        public override Type ResultType
        {
            get
            {
                return typeof(bool);
            }
        }

        ActionConditionalOperation()
        {
        }

        public ActionConditionalOperation(ActionConditionalComparator comparator, ActionConditionalOperand operandA, ActionConditionalOperand operandB)
        {
            if (comparator == null)
            {
                throw new ArgumentNullException(nameof(comparator));
            }
            if (operandA == null)
            {
                throw new ArgumentNullException(nameof(operandA));
            }
            if (operandB == null)
            {
                throw new ArgumentNullException(nameof(operandB));
            }

            Comparator = comparator;
            OperandA = operandA;
            OperandB = operandB;

            if (operandA.ResultType != operandB.ResultType)
            {
                operandBConversion = MakeConversion(operandB.ResultType, operandA.ResultType);
                if (operandB is Constant)
                {
                    var convertedValue = operandBConversion(operandB.ProvideValue(null));
                    operandB = new Constant(convertedValue);
                    operandBConversion = null;
                }
            }
        }

        public override object ProvideValue(ISession session)
        {
            if (this == True)
            {
                return true;
            }

            if (Comparator == null || OperandA == null || OperandB == null)
            {
                throw new InvalidOperationException("Unable to provide value for ActionConditionOperation because either Comparator, OperandA or OperandB is null.");
            }

            var a = OperandA.ProvideValue(session);
            var b = OperandB.ProvideValue(session);

            if (operandBConversion != null)
            {
                b = operandBConversion(b);
            }

            return Comparator.Compare(a, b);
        }

        Func<object, object> MakeConversion(Type from, Type to)
        {
            if (to.IsEnum)
            {
                var inputParameter = Expression.Parameter(typeof(object));
                var asStringConversion = typeof(ConversionExtensions).GetMethod("AsString");
                var trueConstant = Expression.Constant(true, typeof(bool));
                var toConstant = Expression.Constant(to, typeof(Type));
                var enumParse = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });

                var stringConversion = Expression.Call(asStringConversion, inputParameter);
                var parse = Expression.Call(enumParse, toConstant, stringConversion, trueConstant);
                var conversion = Expression.Convert(parse, typeof(object));

                var lambda = LambdaExpression.Lambda<Func<object, object>>(conversion, inputParameter);
                return lambda.Compile();
            }
            throw new NotImplementedException($"Unable to create converson from type {from.Name} to {to.Name}");
        }
    }
}
