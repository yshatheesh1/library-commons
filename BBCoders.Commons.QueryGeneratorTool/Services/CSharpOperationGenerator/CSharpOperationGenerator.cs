using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public abstract class CSharpOperationGenerator : OperationGenerator
    {
        private const string _modelSuffix = "Model";

        protected CSharpOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable table) : base(dependencies, table)
        {
        }



        protected string GetInputModelName()
        {
            return GetEntityName().Pascalize().Singularize() + "Key";
        }

        protected string GetOutputModelName()
        {
            return GetEntityName().Pascalize().Singularize() + "Model";
        }

        protected void GenerateInputModel(IndentedStringBuilder builder)
        {
            var properties = _table.PrimaryKey.Columns.Select(x => x.PropertyMappings.First().Property);
            builder.AppendLine($"public class {GetInputModelName()}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var property in properties)
                {
                    builder.Append($"public {getTypeName(property)} {property.Name}")
                            .AppendLine(" { get; set; }");
                }
            }
            builder.AppendLine("}");
        }

        protected void GenerateOutputModel(IndentedStringBuilder builder)
        {
            var properties = _table.Columns.Select(x => x.PropertyMappings.First().Property);
            builder.AppendLine($"public class {GetOutputModelName()}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var property in properties)
                {
                    builder.Append($"public {getTypeName(property)} {property.Name}")
                            .AppendLine(" { get; set; }");
                }
            }
            builder.AppendLine("}");
        }

        protected void GenerateBaseMethod(IndentedStringBuilder builder, MethodOperation methodOperation)
        {
            var (returnType, methodName, inputModel) = (
                methodOperation.HasResult ? methodOperation.OutputModel : "int",
                methodOperation.MethodName,
                methodOperation.InputModel);
            builder.AppendLine($"public static async Task<{returnType}> {methodName}(this DbConnection connection, {inputModel} {inputModel.Camelize()}, DbTransaction transaction = null, int? timeout = null)");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine($"string sql = @\"{methodOperation.Sql}\";");
                builder.AppendLine("var command = connection.CreateCommand(sql, transaction, timeout);");
                foreach (var parameter in methodOperation.InputModelParameters)
                {
                    var dbType = parameter.DbType != null ? "DbType." + parameter.DbType : "null";
                    builder.AppendLine($"command.CreateParameter(\"@{parameter.Name}\", {inputModel.Camelize()}.{parameter.Name});");
                }
                builder.AppendLine("if (connection.State == ConnectionState.Closed)");
                using (builder.Indent())
                {
                    builder.AppendLine("await connection.OpenAsync();");
                }
                if (!methodOperation.HasResult)
                {
                    builder.AppendLine("return await command.ExecuteNonQueryAsync();");
                }
                else
                {
                    if (methodOperation.UpdateInputModel)
                    {
                        builder.AppendLine($"return await Get{GetEntityName()}ResultSet(command, {inputModel.Camelize()});");
                    }
                    else
                    {
                        builder.AppendLine($"return await Get{GetEntityName()}ResultSet(command);");
                    }

                }
            }
            builder.AppendLine("}");
        }

        protected void GenerateResultSetMethod(IndentedStringBuilder builder)
        {
            var modelName = GetOutputModelName();
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            builder.AppendLine($"private static async Task<{modelName}> Get{GetEntityName()}ResultSet(DbCommand cmd, {modelName} result = null)");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine("var reader = await cmd.ExecuteReaderAsync();");
                builder.AppendLine("while (await reader.ReadAsync())");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    builder.AppendLine($"if(result == null) result = new {modelName}();");
                    foreach (var property in properties.Keys)
                    {
                        builder.Append($"result.{property.Name} = ");
                        if (property.IsNullable)
                        {
                            builder.Append($"Convert.IsDBNull(reader[\"{properties[property].Name}\"]) ? null : ");
                        }
                        builder.AppendLine($"({getTypeName(property)})reader[\"{properties[property].Name}\"];");
                    }
                }
                builder.AppendLine("}");
                builder.AppendLine("reader.Close();");
                builder.AppendLine("return result;");
            }
            builder.AppendLine("}");
        }
    }
}