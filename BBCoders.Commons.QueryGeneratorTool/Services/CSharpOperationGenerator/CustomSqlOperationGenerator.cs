using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class CustomSqlOperationGenerator : IOperationGenerator
    {
        private const string _modelSuffix = "SelectModel";
        private readonly SqlModel customSqlModel;

        public CustomSqlOperationGenerator(SqlModel customSqlModel)
        {
            this.customSqlModel = customSqlModel;
        }

        public void GenerateModel(IndentedStringBuilder builder)
        {
            var name = PascalCase(customSqlModel.MethodName);
            var requestModelName = name + "RequestModel";

            builder.AppendLine($"public class {requestModelName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var parameter in customSqlModel.EqualBindings)
                {
                    if (!parameter.hasDefault)
                    {
                        builder.Append($"public {parameter.Type} {parameter.Value}")
                                .AppendLine(" { get; set; }");
                    }
                }
                foreach (var parameter in customSqlModel.InBindings)
                {
                    builder.Append($"public {parameter.Type} {parameter.Value}")
                            .AppendLine(" { get; set; }");
                }
            }
            builder.AppendLine("}");
            var responseModelName = name + "ResponseModel";
            builder.AppendLine($"public class {responseModelName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                var projections = customSqlModel.Projections.Where(x => x.Table == null).ToList();
                var models = customSqlModel.Projections.Where(x => x.Table != null).GroupBy(x => x.Table.Name).ToDictionary(x => x.Key, y => y.ToList());

                foreach (var model in models)
                {
                    var className = model.Key.Singularize().Pascalize();
                    builder.Append($"public {className}Projection {className}")
                           .AppendLine(" { get; set; }");
                }

                builder.AppendLine($"public {responseModelName}()");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    foreach (var model in models)
                    {
                        var className = model.Key.Singularize().Pascalize();
                        builder.Append($"{className}")
                        .AppendLine($" = new {className}Projection();");
                    }
                }
                builder.AppendLine("}");

                foreach (var model in models)
                {
                    var className = model.Key.Singularize().Pascalize();
                    builder.AppendLine($"public class {className}Projection");
                    builder.AppendLine("{");
                    using (builder.Indent())
                    {
                        renderModelProjections(model.Value, builder);
                    }
                    builder.AppendLine("}");
                }
                renderModelProjections(projections, builder);
            }
            builder.AppendLine("}");
        }
        public void GenerateMethod(IndentedStringBuilder builder)
        {
            var name = PascalCase(customSqlModel.MethodName);
            var requestModel =  name + "RequestModel";
            var requestModelName = requestModel.Camelize();
            var responseModelName = name + "ResponseModel";
            builder.AppendLine($"public static async Task<List<{responseModelName}>> {name}(this DbConnection connection, {requestModel} {requestModelName}, DbTransaction transaction = null, int? timeout = null)");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var property in customSqlModel.InBindings)
                {
                    var propertyName = $"{property.Name.Pluralize()}Joined";
                    builder.AppendLine($"var {propertyName} = string.Join(\",\", {requestModelName}?.{property.Name}.Select((x,y) => \"@{property.Name}\" + y.ToString()).ToArray());");
                }
                var sqlBuilder = new IndentedStringBuilder();
                GenerateSql(sqlBuilder);
                builder.AppendLine(sqlBuilder.ToString());
                builder.AppendLine("var command = connection.CreateCommand(sql, transaction, timeout);");
                foreach (var property in customSqlModel.EqualBindings)
                {
                    if (property.hasDefault)
                    {
                        builder.AppendLine($"command.CreateParameter(\"{property.Name}\", {property.DefaultValue});");
                    }
                    else
                    {
                        builder.AppendLine($"command.CreateParameter(\"{property.Name}\", {requestModelName}.{property.Value});");
                    }
                }
                foreach (var property in customSqlModel.InBindings)
                {
                    var propertyName = $"{property.Name.Pluralize()}Parameters";
                    builder.AppendLine($"{requestModelName}?.{property.Name}.Select((x,y) => command.CreateParameter(\"@{property.Name}\" + y.ToString(), x)).ToArray();");
                }
                builder.AppendLine($"List<{responseModelName}> results = new List<{responseModelName}>();");
                builder.AppendLine("var reader = await command.ExecuteReaderAsync();");
                builder.AppendLine("while (await reader.ReadAsync())");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    builder.AppendLine($"{responseModelName} result = new {responseModelName}();");
                    for (var i = 0; i < customSqlModel.Projections.Count; i++)
                    {
                        var property = customSqlModel.Projections[i];
                        var type = property.IsNullable && property.IsValueType ? property.Type + "?" : property.Type;
                        var propertyName = property.Table != null ? property.Table.Name.Singularize().Pascalize() + "." + property.Name : property.Name;
                        builder.Append($"result.{propertyName} = ");
                        if (property.IsNullable)
                            builder.Append($"Convert.IsDBNull(reader[{i}]) ? null : ");
                        builder.AppendLine($"({type})reader[{i}];");
                    }
                    builder.AppendLine("results.Add(result);");
                }
                builder.AppendLine("}");
                builder.AppendLine("reader.Close();");
                builder.AppendLine("return results;");
            }
            builder.AppendLine("}");
        }
        string PascalCase(string name)
        {
            return name.Pascalize();
        }

        public void GenerateSql(IndentedStringBuilder migrationCommandListBuilder)
        {
            var sql = customSqlModel.Sql.Replace("\n", "\n\t\t\t\t");
            Regex rgx = new Regex(@"\s*IN\s*\(.*?\)");
            int index = 0;
            foreach (var property in customSqlModel.InBindings)
            {
                var propertyName = $"{property.Name.Pluralize()}Joined";
                var match = rgx.Match(sql, index);
                if (match.Success)
                {
                    var inClause = $" IN (\" + {propertyName} + @\")";
                    sql = sql.Replace(match.Value, inClause);
                    index = match.Index + inClause.Length;
                }
            }
            migrationCommandListBuilder.AppendLine($"string sql = @\"{sql}\";");
        }

        void renderModelProjections(List<SqlProjection> projections, IndentedStringBuilder builder)
        {
            foreach (var parameter in projections)
            {
                var type = parameter.IsNullable && parameter.IsValueType ? parameter.Type + "?" : parameter.Type;
                builder.Append($"public {type} {parameter.Name}")
                        .AppendLine(" { get; set; }");
            }
        }
    }

}