using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace BBCoders.Commons.Tools.src.QueryGenerator.Helpers
{
    public class ParameterExpressionHelper
    {
        public static (SortedDictionary<int, ParameterExpression>, object[]) GetParameters(String expression, List<ParameterExpression> parameters)
        {
            var sortedParameters = new SortedDictionary<int, ParameterExpression>();
            var bindings = new object[parameters.Count];
            var trimmedExpression = Regex.Replace(expression, @"\s+", "");
            var trimmedCharExpression = trimmedExpression.ToCharArray();
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var m = trimmedExpression.IndexOf(parameter.Name);

                if (m != -1 && (m == 0 || trimmedCharExpression[m - 1] != '.'))
                {
                    sortedParameters.Add(m, parameter);
                }
                var val = RandomValue(parameter.Type);
                bindings[i] = val;
            }
            return (sortedParameters, bindings);
        }
        
        private static object RandomValue(Type type)
        {
            object value = null;
            if (type == typeof(System.String))
            {
                value = "RandomeString";
            }
            else if (type == typeof(System.Guid))
            {
                value = Guid.NewGuid();
            }
            else if (type == typeof(System.DateTime))
            {
                value = DateTime.Now;
            }
            else if (type == typeof(System.Boolean))
            {
                value = true;
            }
            else if (type == typeof(System.Byte))
            {
                value = Byte.MaxValue;
            }
            else if (type == typeof(System.SByte))
            {
                value = SByte.MaxValue;
            }
            else if (type == typeof(System.Char))
            {
                value = 'T';
            }
            else if (type == typeof(System.Decimal))
            {
                value = Decimal.MaxValue;
            }
            else if (type == typeof(System.Double))
            {
                value = Double.MaxValue;
            }
            else if (type == typeof(System.Single))
            {
                value = Single.MaxValue;
            }
            else if (type == typeof(System.Int16))
            {
                value = Int16.MaxValue;
            }
            else if (type == typeof(System.UInt16))
            {
                value = UInt16.MaxValue;
            }
            else if (type == typeof(System.IntPtr))
            {
                value = IntPtr.Zero;
            }
            else if (type == typeof(System.UIntPtr))
            {
                value = UIntPtr.Zero;
            }
            else if (type == typeof(System.Int32))
            {
                value = Int32.MaxValue;
            }
            else if (type == typeof(System.UInt32))
            {
                value = UInt32.MaxValue;
            }
            else if (type == typeof(System.Int64))
            {
                value = Int64.MaxValue;
            }
            else if (type == typeof(System.UInt64))
            {
                value = UInt64.MaxValue;
            }
            else
            {
                throw new Exception("Given type of parameter is not value type - " + type.FullName);
            }
            return value;
        }

    }
}