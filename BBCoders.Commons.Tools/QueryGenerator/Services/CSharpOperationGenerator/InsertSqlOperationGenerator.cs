using System.Linq;
using BBCoders.Commons.Tools.QueryGenerator.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public class InsertSqlOperationGenerator : CSharpOperationGenerator
    {
        public InsertSqlOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable operation) :
        base(dependencies, operation)
        { }


        public override void GenerateModel(IndentedStringBuilder builder)
        {
        }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var table = DelimitTable(_table.Name, _table.Schema, false);
            var columnMappings = GetMappings();
            var autoIncrementColumn = columnMappings.FirstOrDefault(x => x.isAutoIncrement());
            var columns = GetMappings().Where(x => !x.isAutoIncrement());
            var insertColumns = new string[columns.Count()];
            var insertValues = new string[columns.Count()];
            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns.ElementAt(i);
                insertColumns[i] = DelimitColumn(_table.Name, column.Name, false);
                insertValues[i] = column.hasDefaultValue() ? $"If(@{column.Name} IS NULL,DEFAULT({DelimitColumn(_table.Name, column.Name)}), @{column.Name})" : $"@{column.Name}";
            }
            builder.AppendLine($"INSERT INTO {table} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
            if (autoIncrementColumn != null)
                builder.Append($"SELECT * FROM {DelimitTable(_table.Name, _table.Schema, true)} WHERE {DelimitColumn(_table.Name, autoIncrementColumn.Name, true)} = LAST_INSERT_ID()");
            else
                new SelectSqlOperationGenerator(_dependencies, _table).GenerateSql(builder);
        }
        public override void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var tableName = GetEntityName();
            var modelName = getModelName();
            var autoIncrementColumns = GetMappings().Where(x => x.isAutoIncrement()).Select(x => x.Name);
            var autoIncrementColumn = _table.Columns.Where(x => autoIncrementColumns.Contains(x.Name)).FirstOrDefault()?.PropertyMappings.First().Property;
            builder.AppendLine($"public async Task<{modelName}> Insert{tableName}({modelName} {modelName})");
            builder.AppendLine("{");
            var nonAutoIncrementColumn = _table.Columns.Where(x => !autoIncrementColumns.Contains(x.Name));
            var properties = nonAutoIncrementColumn.ToDictionary(x => x.PropertyMappings.First().Property, y => y);

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
                        builder.AppendLine($"cmd.Parameters.AddWithValue(\"@{properties[property].Name}\", {modelName}.{property.Name});");
                    }
                    builder.AppendLine($"return await GetResult(cmd, {modelName});");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");
        }
    }
}