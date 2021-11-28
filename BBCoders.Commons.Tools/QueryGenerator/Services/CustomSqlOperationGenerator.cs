using System.Globalization;
using System.Linq;
using BBCoders.Commons.QueryConfiguration;
using BBCoders.Commons.Tools.QueryGenerator.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public class CustomSqlOperationGenerator : ISqlOperationGenerator
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
                foreach (var parameter in customSqlModel.Bindings)
                {
                    if (!parameter.hasDefault)
                    {
                        builder.Append($"public {parameter.Type} {parameter.Value}")
                                .AppendLine(" { get; set; }");
                    }
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
                    builder.Append($"public {parameter.Type} {parameter.Name}")
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
            builder.AppendLine($"public async Task<{responseModelName}> {name}({requestModelName} {requestModelName})");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                // method implementation
                builder.AppendLine($"using(var connection = new MySqlConnection({connectionString}))");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    builder.AppendLine("await connection.OpenAsync();");
                    builder.Append("string sql = @\"");
                    GenerateSql(builder);
                    builder.AppendLine("\";");
                    builder.AppendLine($"var cmd = new MySqlCommand(sql, connection);");
                    foreach (var property in customSqlModel.Bindings)
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
                    builder.AppendLine($"{responseModelName} result = null;");
                    builder.AppendLine("var reader = await cmd.ExecuteReaderAsync();");
                    builder.AppendLine("while (await reader.ReadAsync())");
                    builder.AppendLine("{");
                    using (builder.Indent())
                    {
                        builder.AppendLine($"result = new {responseModelName}();");
                        foreach (var property in customSqlModel.Projections)
                        {
                            builder.AppendLine($"result.{property.Name} = ({property.Type})reader[\"{property.Name}\"];");
                        }
                    }
                    builder.AppendLine("}");
                    builder.AppendLine("reader.Close();");
                    builder.AppendLine("return result;");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");
        }
        string PascalCase(string name)
        {
            var result = new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();
            return new CultureInfo("en-US", false).TextInfo.ToTitleCase(result);
        }

        public void GenerateSql(IndentedStringBuilder migrationCommandListBuilder)
        {
            migrationCommandListBuilder.Append(customSqlModel.Sql);
        }
    }

}