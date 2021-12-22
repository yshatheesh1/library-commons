using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class UpdateSqlOperationGenerator : CSharpOperationGenerator
    {
        public UpdateSqlOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable operation) :
        base(dependencies, operation)
        { }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
        }

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
            builder.Append($"UPDATE {DelimitTable(_table.Name, _table.Schema, true)} {SetClause(_table.Name, setColumn, setValue, true)} {WhereClause(_table.Name, keyColumns, keyColumnMappings, true)};");
        }

        public override void GenerateMethod(IndentedStringBuilder builder)
        {
            var tableName = GetEntityName();
            var primaryKeys = _table.PrimaryKey.Columns.Select(x => x.Name);
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var sqlBuilder = new IndentedStringBuilder();
            GenerateSql(sqlBuilder);
            var methodOp = new MethodOperation()
            {
                MethodName = "Update" + tableName,
                InputModel = GetOutputModelName(),
                InputModelParameters = properties.Select(x => new ModelParameter { Name = properties[x.Key].Name, Value = x.Key.Name, DbType = _dependencies.relationalTypeMappingSource.FindMapping(x.Key.ClrType).DbType.ToString(), Type = getTypeName(x.Key) }).ToList(),
                Sql = sqlBuilder.ToString(),
                HasResult = false,
                UpdateInputModel = true
            };
            GenerateBaseMethod(builder, methodOp);
        }
    }
}