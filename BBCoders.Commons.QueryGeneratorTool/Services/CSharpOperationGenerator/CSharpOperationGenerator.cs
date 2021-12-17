using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
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

        public override void GenerateModel(IndentedStringBuilder builder)
        {
            var properties = _table.Columns.Select(x => x.PropertyMappings.First().Property);
            builder.AppendLine($"public class {getModelName()}");
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

        protected string GetResultSetMethodName()
        {
            var modelName = GetEntityName();
            return $"Get{modelName}ResultSet";
        }

        protected void GetResultSetMethod(IndentedStringBuilder builder)
        {
            var modelName = getModelName();
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            builder.AppendLine($"private async Task<{modelName}> {GetResultSetMethodName()}(MySqlCommand cmd, {modelName} result = null)");
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
        protected string getModelName()
        {
            return GetEntityName() + _modelSuffix; ;
        }
    }
}