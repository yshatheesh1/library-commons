using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class DeleteSqlOperationGenerator : CSharpOperationGenerator
    {
        public DeleteSqlOperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable table) :
        base(dependencies, table)
        { }

        public override void GenerateSql(IndentedStringBuilder builder)
        {
            var keyColumns = GetMappings().Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            var columnMappings = columns.Select(x => "@" + x).ToArray();
            builder.Append($"DELETE FROM {DelimitTable(_table.Name, _table.Schema, true)} {WhereClause(_table.Name, columns, columnMappings, true)}");
        }

        public override void GenerateModel(IndentedStringBuilder builder)
        {
            GenerateInputModel(builder);
            GenerateOutputModel(builder);
        }

        public override void GenerateMethod(IndentedStringBuilder builder)
        {
            var tableName = GetEntityName();
            var primaryKeyProperties = _table.PrimaryKey.Columns.ToDictionary(x => x.PropertyMappings.First().Property, y => y);
            var sqlBuilder = new IndentedStringBuilder();
            GenerateSql(sqlBuilder);
            var methodOp = new MethodOperation()
            {
                MethodName = "Delete" + tableName,
                InputModel = GetInputModelName(),
                InputModelParameters = primaryKeyProperties.Select(x => new ModelParameter { Name = primaryKeyProperties[x.Key].Name, Value = x.Key.Name, DbType = _dependencies.relationalTypeMappingSource.FindMapping(x.Key.ClrType).DbType.ToString(), Type = getTypeName(x.Key) }).ToList(),
                Sql = sqlBuilder.ToString(),
                HasResult = false
            };
            GenerateBaseMethod(builder, methodOp);

            GenerateResultSetMethod(builder);
        }
    }
}