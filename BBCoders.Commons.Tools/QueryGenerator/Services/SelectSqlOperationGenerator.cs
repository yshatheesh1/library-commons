using System.Linq;
using BBCoders.Commons.Tools.QueryGenerator.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public class SelectSqlOperationGenerator : SqlOperationGenerator
    {
        private const string _modelSuffix = "SelectModel";
        public SelectSqlOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable operation) :
        base(dependencies, operation)
        { }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var parameters = GetMappings();
            var keyColumns = parameters.Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            var columnMappings = columns.Select(x => "@" + x).ToArray();
            var selectColumns = parameters.Select(x => DelimitColumn(_table.Name, x.Name, true));
            builder.AppendLine($"SELECT {string.Join(",", selectColumns)} ");
            builder.Append("FROM ");
            builder.AppendLine(DelimitTable(_table.Name, _table.Schema, true));
            builder.Append(WhereClause(_table.Name, columns, columnMappings, true));
        }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
            var selectModelName = GetEntityName() + _modelSuffix;
            var properties = _table.Columns.Select(x => x.PropertyMappings.First().Property);
            GenerateModel(builder, selectModelName, properties);
        }
        public override void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var tableName = GetEntityName();
            var modelName = tableName + "SelectModel";
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var primaryKeyProperties = _table.PrimaryKey.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var inputs = string.Join(", ", primaryKeyProperties.Keys.Select(x => getTypeName(x) + " " + x.Name));
            builder.AppendLine($"public async Task<{modelName}> Select{tableName}({inputs})");
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
                    foreach (var property in primaryKeyProperties.Keys)
                    {
                        builder.AppendLine($"cmd.Parameters.AddWithValue(\"@{primaryKeyProperties[property].Name}\", {property.Name});");
                    }
                    builder.AppendLine($"{modelName} result = null;");
                    builder.AppendLine("var reader = await cmd.ExecuteReaderAsync();");
                    builder.AppendLine("while (await reader.ReadAsync())");
                    builder.AppendLine("{");
                    using (builder.Indent())
                    {
                        builder.AppendLine($"result = new {modelName}();");
                        foreach (var property in properties.Keys)
                        {
                            builder.AppendLine($"result.{property.Name} = ({getTypeName(property)})reader[\"{properties[property].Name}\"];");
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
    }
}