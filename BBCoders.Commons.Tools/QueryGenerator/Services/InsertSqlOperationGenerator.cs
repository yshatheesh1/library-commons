using System.Linq;
using BBCoders.Commons.QueryConfiguration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public class InsertSqlOperationGenerator : SqlOperationGenerator
    {
        private const string _modelSuffix = "InsertModel";
        public InsertSqlOperationGenerator(ISqlGenerationHelper sqlGenerationHelper, ITable operation) :
        base(sqlGenerationHelper, operation)
        { }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var table = DelimitTable(_table.Name, _table.Schema, false);
            var columns = GetMappings().Where(x => !x.isAutoIncrement());
            var insertColumns = new string[columns.Count()];
            var insertValues = new string[columns.Count()];
            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns.ElementAt(i);
                insertColumns[i] = DelimitColumn(_table.Name, column.Name, false);
                insertValues[i] = column.hasDefaultValue() ? $"If({column.Name} IS NULL,DEFAULT({DelimitColumn(_table.Name, column.Name)}), {column.Name})" : column.Name;
            }
            builder.AppendLine($"INSERT INTO {table} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
            builder.Append("SELECT LAST_INSERT_ID()");
        }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
            var insertModelName = GetEntityName() + _modelSuffix;
            var columns = GetMappings().Where(x => !x.isAutoIncrement()).Select(x => x.Name);
            var properties = _table.Columns.Where(x => columns.Contains(x.Name)).Select(x => x.PropertyMappings.First().Property);
            GenerateModel(builder, insertModelName, properties);
        }

        public override void GenerateMethod(IndentedStringBuilder builder, string connectionString)
        {
            var modelName = GetEntityName();
            var insertModelName = modelName + _modelSuffix;

            var columns = GetMappings().Where(x => x.isAutoIncrement()).Select(x => x.Name);
            var autoIncrementColumn = _table.Columns.Where(x => columns.Contains(x.Name)).FirstOrDefault();
            var returnType = autoIncrementColumn != null ? getTypeName(autoIncrementColumn.PropertyMappings.First().Property.ClrType) : "int";
            builder.AppendLine($"public async Task<{returnType}> Insert{modelName}({insertModelName} {insertModelName})");
            builder.AppendLine("{");
            var nonAutoIncrementColumn = _table.Columns.Where(x => x != autoIncrementColumn);
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
                        builder.AppendLine($"cmd.Parameters.AddWithValue(\"@{properties[property].Name}\", {insertModelName}.{property.Name});");
                    }
                    if (autoIncrementColumn != null)
                    {
                        builder.AppendLine($"return Convert.To{returnType}(await cmd.ExecuteScalarAsync());");
                    }
                    else
                    {
                        builder.AppendLine("return await cmd.ExecuteNonQueryAsync();");
                    }
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");
        }
    }
}