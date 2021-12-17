using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using BBCoders.Commons.Utilities;

namespace BBCoders.Commons.QueryGeneratorTool.Helpers
{
    public class BindingParameter
    {
        public object DefaultValue { get; set; }
        public ParameterExpression Expression { get; set; }
        public bool InExpression { get; set; }
    }
    public class ParameterExpressionHelper
    {
        public static (SortedDictionary<int, BindingParameter>, object[]) GetParameters(String expression, List<ParameterExpression> parameters)
        {
            var sortedParameters = new SortedDictionary<int, BindingParameter>();
            var bindings = new object[parameters.Count];
            var trimmedExpression = Regex.Replace(expression, @"\s+", "");
            var trimmedCharExpression = trimmedExpression.ToCharArray();
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var defaultValue = RandomValue(parameter.Type);
                var bindingParameter = new BindingParameter()
                {
                    Expression = parameter,
                    DefaultValue = defaultValue.Item1,
                    InExpression = defaultValue.Item2
                };
                var m = IndexOf(trimmedExpression, parameter.Name);
                if (m != -1)
                {
                    sortedParameters.Add(m, bindingParameter);
                }
                bindings[i] = bindingParameter.DefaultValue;
            }
            return (sortedParameters, bindings);
        }

        private static int IndexOf(string str, string value)
        {
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1 || (index == 0 || str[index - 1] != '.'))
                    return index;
            }
        }

        private static (object, bool) RandomValue(Type type, bool isList = false)
        {
            object value = null;
            if (type == typeof(System.String))
            {
                var randomeValue = "RandomeString";
                if (isList)
                {
                    value = new List<string>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Guid))
            {
                var randomeValue = Guid.NewGuid();
                if (isList)
                {
                    value = new List<Guid>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.DateTime))
            {
                var randomeValue = DateTime.Now;
                if (isList)
                {
                    value = new List<DateTime>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Boolean))
            {
                var randomeValue = true;
                if (isList)
                {
                    value = new List<Boolean>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Byte))
            {
                var randomeValue = Byte.MaxValue;
                if (isList)
                {
                    value = new List<Byte>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.SByte))
            {
                var randomeValue = SByte.MaxValue;
                if (isList)
                {
                    value = new List<SByte>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Char))
            {
                var randomeValue = 'T';
                if (isList)
                {
                    value = new List<Char>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Decimal))
            {
                var randomeValue = Decimal.MaxValue;
                if (isList)
                {
                    value = new List<Decimal>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Double))
            {
                var randomeValue = Double.MaxValue;
                if (isList)
                {
                    value = new List<Double>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Single))
            {
                var randomeValue = Single.MaxValue;
                if (isList)
                {
                    value = new List<Single>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Int16))
            {
                var randomeValue = Int16.MaxValue;
                if (isList)
                {
                    value = new List<Int16>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.UInt16))
            {
                var randomeValue = UInt16.MaxValue;
                if (isList)
                {
                    value = new List<UInt16>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.IntPtr))
            {
                var randomeValue = IntPtr.Zero;
                if (isList)
                {
                    value = new List<IntPtr>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.UIntPtr))
            {
                var randomeValue = UIntPtr.Zero;
                if (isList)
                {
                    value = new List<UIntPtr>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.Int32))
            {
                var randomeValue = Int32.MaxValue;
                if (isList)
                {
                    value = new List<Int32>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.UInt32))
            {
                var randomeValue = UInt32.MaxValue;
                if (isList)
                {
                    value = new List<UInt32>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }

            }
            else if (type == typeof(System.Int64))
            {
                var randomeValue = Int64.MaxValue;
                if (isList)
                {
                    value = new List<Int64>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else if (type == typeof(System.UInt64))
            {
                var randomeValue = UInt64.MaxValue;
                if (isList)
                {
                    value = new List<UInt64>() { randomeValue, randomeValue };
                }
                else
                {
                    value = randomeValue;
                }
            }
            else
            {
                // complex type validation
                var genericEnumerableType = GetGenericType(type, typeof(IEnumerable<>));
                var genericListType = GetGenericType(type, typeof(IList<>));
                if (genericEnumerableType != null)
                {
                    (value, isList) = RandomValue(genericEnumerableType, true);
                }
                else if (genericListType != null)
                {
                    (value, isList) = RandomValue(genericListType, true);
                }
                else
                {
                    Reporter.WriteError("Given type not supported - " + nameof(type));
                    throw new Exception("Given type not supported - " + nameof(type));
                }
            }
            return (value, isList);
        }

        static Type GetGenericType(Type type, Type enumerableType)
        {
            if (type.IsInterface && type.GetGenericTypeDefinition() == enumerableType)
                return type.GetGenericArguments()[0];
            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsGenericType
                    && intType.GetGenericTypeDefinition() == enumerableType)
                {
                    return intType.GetGenericArguments()[0];
                }
            }
            return null;
        }

    }
}