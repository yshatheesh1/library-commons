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
        public override void GenerateMethod(IndentedStringBuilder builder)
        {
            var tableName = GetEntityName();
            var properties = _table.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var primaryKeyProperties = _table.PrimaryKey.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var inputs = string.Join(", ", primaryKeyProperties.Keys.Select(x => getTypeName(x) + " " + x.Name));
            var sqlBuilder = new IndentedStringBuilder();
            GenerateSql(sqlBuilder);
            var methodOp = new MethodOperation()
            {
                MethodName = "Select" + tableName,
                InputModel = GetInputModelName(),
                InputModelParameters = primaryKeyProperties.Select(x => new ModelParameter { Name = properties[x.Key].Name, Value = x.Key.Name, DbType = _dependencies.relationalTypeMappingSource.FindMapping(x.Key.ClrType).DbType.ToString(), Type = getTypeName(x.Key) }).ToList(),
                Sql = sqlBuilder.ToString(),
                HasResult = true,
                UpdateInputModel = false,
                OutputModel = GetOutputModelName(),
                OutputModelParameters = properties.Select(x => new ModelParameter { Name = x.Key.Name, Value = properties[x.Key].Name, DbType = _dependencies.relationalTypeMappingSource.FindMapping(x.Key.ClrType).DbType.ToString(), Type = getTypeName(x.Key) }).ToList(),
            };
            GenerateBaseMethod(builder, methodOp);
        }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
             
        }
    }
}