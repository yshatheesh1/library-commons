using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
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
        public override void GenerateMethod(IndentedStringBuilder builder)
        {
            var tableName = GetEntityName();
            var autoIncrementColumns = GetMappings().Where(x => x.isAutoIncrement()).Select(x => x.Name);
            var autoIncrementColumn = _table.Columns.Where(x => autoIncrementColumns.Contains(x.Name)).FirstOrDefault()?.PropertyMappings.First().Property;
            var nonAutoIncrementColumn = _table.Columns.Where(x => !autoIncrementColumns.Contains(x.Name));
            var nonAutoIncrementProperties = nonAutoIncrementColumn.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var sqlBuilder = new IndentedStringBuilder();
            GenerateSql(sqlBuilder);
            var methodOp = new MethodOperation()
            {
                MethodName = "Insert" + tableName,
                InputModel = GetOutputModelName(),
                InputModelParameters = nonAutoIncrementProperties.Select(x => new ModelParameter { Name = properties[x.Key].Name, Value = x.Key.Name, DbType = _dependencies.relationalTypeMappingSource.FindMapping(x.Key.ClrType).DbType.ToString(), Type = getTypeName(x.Key) }).ToList(),
                Sql = sqlBuilder.ToString(),
                HasResult = true,
                UpdateInputModel = true,
                OutputModel = GetOutputModelName(),
                OutputModelParameters = properties.Select(x => new ModelParameter { Name = x.Key.Name, Value = properties[x.Key].Name, DbType = _dependencies.relationalTypeMappingSource.FindMapping(x.Key.ClrType).DbType.ToString(), Type = getTypeName(x.Key) }).ToList(),
            };
            GenerateBaseMethod(builder, methodOp);
        }
    }
}