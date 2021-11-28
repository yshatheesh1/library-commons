using System.Linq;
using BBCoders.Commons.QueryConfiguration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public class DeleteSqlOperationGenerator : SqlOperationGenerator
    {

        private const string _modelSuffix = "DeleteModel";
        public DeleteSqlOperationGenerator(ISqlGenerationHelper sqlGenerationHelper, ITable table) :
        base(sqlGenerationHelper, table)
        { }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var keyColumns = GetMappings().Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            builder.AppendLine($"DELETE FROM {DelimitTable(_table.Name, _table.Schema, true)}");
            builder.Append(WhereClause(_table.Name, columns, columns, true));
        }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
            var deleteModelName = GetEntityName() + _modelSuffix;
            var properties = _table.PrimaryKey.Columns.Select(x => x.PropertyMappings.First().Property);
            GenerateModel(builder, deleteModelName, properties);
        }

        public override void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var modelName = GetEntityName();
            var deleteModelName = modelName + _modelSuffix;
            var primaryKeyProperties = _table.PrimaryKey.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var inputs = string.Join(", ", primaryKeyProperties.Keys.Select(x => getTypeName(x.ClrType) + " " + x.Name));
            builder.AppendLine($"public async Task<int> Delete{modelName}({inputs})");
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
                    builder.AppendLine("return await cmd.ExecuteNonQueryAsync();");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");
        }
    }
}