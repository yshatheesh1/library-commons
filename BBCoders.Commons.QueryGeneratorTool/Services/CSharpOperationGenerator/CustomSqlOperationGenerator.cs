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
                foreach (var parameter in customSqlModel.Projections)
                {
                    var type = parameter.IsNullable && parameter.IsValueType ? parameter.Type + "?" : parameter.Type;
                    builder.Append($"public {type} {parameter.Name}")
                            .AppendLine(" { get; set; }");
                }
            }
            builder.AppendLine("}");
        }
        public void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var name = PascalCase(customSqlModel.MethodName);
            var requestModelName = name + "RequestModel";
            var responseModelName = name + "ResponseModel";
            builder.AppendLine($"public async Task<List<{responseModelName}>> {name}({requestModelName} {requestModelName})");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var property in customSqlModel.InBindings)
                {
                    var propertyName = $"{property.Name.Pluralize()}Joined";
                    builder.AppendLine($"var {propertyName} = string.Join(\",\", {requestModelName}?.{property.Name}.Select((x,y) => \"@{property.Name}\" + y.ToString()).ToArray());");
                }
                GenerateSql(builder);
                builder.AppendLine($"using(var connection = new MySqlConnection({connectionString}))");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    builder.AppendLine("await connection.OpenAsync();");
                    builder.AppendLine($"var cmd = new MySqlCommand(sql, connection);");
                    foreach (var property in customSqlModel.EqualBindings)
                    {
                        if (property.hasDefault)
                        {
                            builder.AppendLine($"cmd.Parameters.AddWithValue(\"{property.Name}\", {property.DefaultValue});");
                        }
                        else
                        {
                            builder.AppendLine($"cmd.Parameters.AddWithValue(\"{property.Name}\", {requestModelName}.{property.Value});");
                        }
                    }
                    foreach (var property in customSqlModel.InBindings)
                    {
                        var propertyName = $"{property.Name.Pluralize()}Parameters";
                        builder.AppendLine($"var {propertyName} = {requestModelName}?.{property.Name}.Select((x,y) => new MySqlParameter(\"@{property.Name}\" + y.ToString(), x)).ToArray();");
                        builder.AppendLine($"cmd.Parameters.AddRange({propertyName});");
                    }
                    builder.AppendLine($"List<{responseModelName}> results = new List<{responseModelName}>();");
                    builder.AppendLine("var reader = await cmd.ExecuteReaderAsync();");
                    builder.AppendLine("while (await reader.ReadAsync())");
                    builder.AppendLine("{");
                    using (builder.Indent())
                    {
                        builder.AppendLine($"{responseModelName} result = new {responseModelName}();");
                        for (var i = 0; i < customSqlModel.Projections.Count; i++)
                        {
                            var property = customSqlModel.Projections[i];
                            var type = property.IsNullable && property.IsValueType ? property.Type + "?" : property.Type;
                            builder.Append($"result.{property.Name} = ");
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
    }

}