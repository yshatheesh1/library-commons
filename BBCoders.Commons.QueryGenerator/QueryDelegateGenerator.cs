using System.Collections.Generic;
using BBCoders.Commons.SourceGenerator;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.QueryGenerator
{
    internal class QueryDelegateGenerator : ISourceGenerator
    {
        public void Execute(SourceContext context)
        {
            context.Add(CreateQueryDelegateSource(), "QueryDelegates");
            context.Add(CreateQueryExtensionsSource(), "QueryOperationsExtensions");
        }

        public string CreateQueryDelegateSource()
        {
            var builder = new IndentedStringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine();
            builder.AppendLine("namespace BBCoders.Commons.QueryGenerator");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                // generate func with more than default 16 parameters
                var funcGenericExpression = "in T1";
                var funcExpression = "T1 t1";
                var builderNested1 = new List<string>();
                builderNested1.Add("/// <summary>");
                builderNested1.Add("/// lambda for adding query operation");
                builderNested1.Add("/// </summary>");
                builderNested1.Add("/// <param name=\"t1\"></param>");
                builderNested1.Add("/// <param name=\"t2\"></param>");
                builderNested1.Add("/// <param name=\"t3\"></param>");
                builderNested1.Add("/// <param name=\"t4\"></param>");
                builderNested1.Add("/// <param name=\"t5\"></param>");
                builderNested1.Add("/// <param name=\"t6\"></param>");
                builderNested1.Add("/// <param name=\"t7\"></param>");
                builderNested1.Add("/// <param name=\"t8\"></param>");
                builderNested1.Add("/// <param name=\"t9\"></param>");
                builderNested1.Add("/// <param name=\"t10\"></param>");
                builderNested1.Add("/// <param name=\"t11\"></param>");
                builderNested1.Add("/// <param name=\"t12\"></param>");
                builderNested1.Add("/// <param name=\"t13\"></param>");
                builderNested1.Add("/// <param name=\"t14\"></param>");
                builderNested1.Add("/// <param name=\"t15\"></param>");
                builderNested1.Add("/// <param name=\"t16\"></param>");
                builderNested1.Add("/// <param name=\"t17\"></param>");
                var builderNested2 = new List<string>();
                builderNested2.Add("/// <typeparam name=\"T1\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T2\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T3\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T4\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T5\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T6\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T7\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T8\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T9\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T10\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T11\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T12\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T13\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T14\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T15\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T16\"></typeparam>");
                builderNested2.Add("/// <typeparam name=\"T17\"></typeparam>");

                for (var i = 1; i < 100; i++)
                {
                    if (i > 16)
                    {
                        builderNested1.ForEach(x => builder.AppendLine(x));
                        builderNested2.ForEach(x => builder.AppendLine(x));
                        builder.AppendLine("/// <typeparam name=\"TResult\"></typeparam>");
                        builder.AppendLine("/// <returns></returns>");
                        builder.AppendLine($"public delegate TResult Func<{funcGenericExpression}, out TResult>({funcExpression});");
                        builderNested1.Add($"/// <param name=\"t{i + 1}\"></param>");
                        builderNested2.Add($"/// <typeparam name=\"T{i + 1}\"></typeparam>");
                    }
                    funcGenericExpression += $", in T{i + 1}";
                    funcExpression += $", T{i + 1} t{i + 1}";
                }
            }
            builder.AppendLine("}");
            return builder.ToString();
        }

        public string CreateQueryExtensionsSource()
        {
            var builder = new IndentedStringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using System.Linq.Expressions;");
            builder.AppendLine();
            builder.AppendLine("namespace BBCoders.Commons.QueryGenerator");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine("/// <summary>");
                builder.AppendLine("/// query operations");
                builder.AppendLine("/// </summary>");
                builder.AppendLine("public static class QueryOperationExtensions");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    var s = new List<string>();
                    s.Add("/// <summary>");
                    s.Add("/// Add query operation");
                    s.Add("/// </summary>");
                    s.Add("/// <param name=\"queryContext\">query context</param>");
                    s.Add("/// <param name=\"name\">name of the operation </param>");
                    s.Add("/// <param name=\"expression\">expression</param> ");
                    s.Add("/// <typeparam name=\"T1\">type of input param </typeparam>");
                    var genericExpression = "T1";
                    for (var i = 1; i < 100; i++)
                    {
                        s.ForEach(x => builder.AppendLine(x));
                        builder.AppendLine($"public static void Add<{genericExpression}>(this QueryContext queryContext, string name, Expression<Func<{genericExpression}, IQueryable>> expression) => queryContext.Add(name, expression.Parameters.ToList(), expression);");
                        genericExpression += $",T{i + 1}";
                        s.Add($"/// <typeparam name=\"T{i + 1}\">type of input param </typeparam>");
                    }
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");
            return builder.ToString();
        }
    }
}