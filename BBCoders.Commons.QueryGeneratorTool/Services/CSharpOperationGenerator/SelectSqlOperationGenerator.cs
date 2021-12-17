using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class SelectSqlOperationGenerator : CSharpOperationGenerator
    {
        public SelectSqlOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable operation) :
        base(dependencies, operation)
        { }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var parameters = GetMappings();
            var keyColumns = parameters.Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            var columnMappings = columns.Select(x => "@" + x).ToArray();
            builder.Append($"SELECT * FROM {DelimitTable(_table.Name, _table.Schema, true)} {WhereClause(_table.Name, columns, columnMappings, true)}");
        }
        public override void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var tableName = GetEntityName();
            var modelName = getModelName();
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
                    builder.AppendLine($"return await {GetResultSetMethodName()}(cmd);");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            // generate Get Result method
            GetResultSetMethod(builder);
        }
    }
}