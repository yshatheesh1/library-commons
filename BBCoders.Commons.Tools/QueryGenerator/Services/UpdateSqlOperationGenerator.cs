using System.Linq;
using BBCoders.Commons.Tools.QueryGenerator.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public class UpdateSqlOperationGenerator : SqlOperationGenerator
    {
        private const string _modelSuffix = "UpdateModel";
        public UpdateSqlOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable operation) :
        base(dependencies, operation)
        { }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var parameters = GetMappings();
            var columns = parameters.Where(x => !x.isPrimaryKey());
            var keyColumns = parameters.Where(x => x.isPrimaryKey()).Select(x => x.Name).ToArray();
            var keyColumnMappings = keyColumns.Select(x => "@" + x).ToArray();
            var setColumn = new string[columns.Count()];
            var setValue = new string[columns.Count()];
            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns.ElementAt(i);
                var delimitColumn = DelimitColumn(_table.Name, column.Name, true);
                setColumn[i] = column.Name;
                setValue[i] = column.hasDefaultValue() ? $"If(@{column.Name} IS NULL,DEFAULT({delimitColumn}), @{column.Name})" : $"@{column.Name}";
            }
            builder.AppendLine($"UPDATE {DelimitTable(_table.Name, _table.Schema, true)}");
            builder.AppendLine(SetClause(_table.Name, setColumn, setValue, true));
            builder.Append(WhereClause(_table.Name, keyColumns, keyColumnMappings, true));
        }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
            var updateModelName = GetEntityName() + _modelSuffix;
            var properties = _table.Columns.Select(x => x.PropertyMappings.First().Property);
            GenerateModel(builder, updateModelName, properties);
        }

        public override void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var tableName = GetEntityName();
            var updateModelName = tableName + "UpdateModel";
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            builder.AppendLine($"public async Task<int> Update{tableName}({updateModelName} {updateModelName})");
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
                    foreach (var property in properties.Keys)
                    {
                        builder.AppendLine($"cmd.Parameters.AddWithValue(\"@{properties[property].Name}\", {updateModelName}.{property.Name});");
                    }
                    builder.AppendLine("return await cmd.ExecuteNonQueryAsync();");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");
        }
    }
}