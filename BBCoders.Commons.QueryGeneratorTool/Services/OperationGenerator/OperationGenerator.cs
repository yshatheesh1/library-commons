using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.QueryGeneratorTool.Helpers;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public abstract class OperationGenerator : IOperationGenerator
    {
        protected ISqlGenerationHelper _sqlGenerationHelper;
        protected IRelationalTypeMappingSource _relationalTypeMappingSource;
        protected SqlOperationGeneratorDependencies _dependencies;
        protected ITable _table;
        public OperationGenerator(SqlOperationGeneratorDependencies dependencies, ITable table)
        {
            _dependencies = dependencies;
            _sqlGenerationHelper = dependencies.sqlGenerationHelper;
            _relationalTypeMappingSource = dependencies.relationalTypeMappingSource;
            _table = table;
        }

        public abstract void GenerateSql(IndentedStringBuilder migrationCommandListBuilder);
        public abstract void GenerateMethod(IndentedStringBuilder builder, string connectionString);

        public abstract void GenerateModel(IndentedStringBuilder builder);

        protected string GenerateParamSentence(IEnumerable<ParameterModel> Parameters)
        {
            return string.Join(", ", Parameters.Select(x => $"IN {x.Name} {BuildParamType(x)}"));
        }

        protected string WhereClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return Clause(table, columns, columnMappings, alias, "WHERE ", "AND ");
        }

        protected string SetClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return Clause(table, columns, columnMappings, alias, "SET ", ", ");
        }

        private string Clause(string table, string[] columns, string[] columnMappings, bool alias, string clause, string delimiter)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(clause);
            for (int i = 0; i < columns.Length; i++)
            {
                stringBuilder.Append(DelimitColumn(table, columns[i], alias));
                stringBuilder.Append(" = ");
                stringBuilder.Append(columnMappings[i]);
                if (i != columns.Length - 1)
                    stringBuilder.Append(delimiter);
            }
            return stringBuilder.ToString();
        }

        protected string DelimitTable(string tableName, string schema, bool alias)
        {
            if (alias)
            {
                var tableAlias = tableName.Substring(0, 1).ToLower();
                return _sqlGenerationHelper.DelimitIdentifier(tableName, schema) + " AS " +
                _sqlGenerationHelper.DelimitIdentifier(tableAlias);
            }
            else
            {
                return _sqlGenerationHelper.DelimitIdentifier(tableName, schema);
            }

        }


        protected string DelimitColumn(string tableName, string column, bool alias)
        {
            var tableAlias = tableName.Substring(0, 1).ToLower();
            return alias ?
                    DelimitColumn(tableAlias, column) :
                    _sqlGenerationHelper.DelimitIdentifier(column);
        }

        protected string DelimitColumn(string tableName, string column)
        {
            return _sqlGenerationHelper.DelimitIdentifier(tableName) + "." +
               _sqlGenerationHelper.DelimitIdentifier(column);
        }

        protected string BuildParamType(ParameterModel param)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(param.Type);
            if (new string[2] { "char", "varchar" }.Contains<string>(param.Type.ToLower()) && param.MaxLength.HasValue)
            {
                stringBuilder.AppendFormat("({0}) ", param.MaxLength.Value);
            }
            if (param.Precision.HasValue && param.Scale.HasValue)
            {
                stringBuilder.AppendFormat("( {0}, {1} ) ", param.Precision.Value, param.Scale.Value);
            }
            return stringBuilder.ToString();
        }

        protected string getTypeName(IProperty property)
        {
            var suffix = property.IsNullable && property.ClrType.IsValueType ? "?" : "";
            var type = _relationalTypeMappingSource.FindMapping(property);
            if (type.DbType.HasValue)
            {
                return SqlMapperHelper.getClrType(type.DbType.Value).Name + suffix;
            }
            return getTypeName(type.ClrType) + suffix;
        }

        protected string getTypeName(Type clrType)
        {
            var nullableType = Nullable.GetUnderlyingType(clrType);
            return nullableType != null ? nullableType.Name + "?" : clrType.Name;
        }

        protected string PascalCase(string name)
        {
            return name.Pascalize();
        }

        protected List<ParameterModel> GetMappings()
        {
            return _table.Columns.Select(column =>
            {
                var property = column.PropertyMappings.First().Property;
                var primaryKey = _table.PrimaryKey.Columns.FirstOrDefault(x => x.Name.Equals(column.Name));
                var isPrimaryKey = primaryKey != null;
                var isIdentity = primaryKey?.PropertyMappings.First().Property.ValueGenerated == ValueGenerated.OnAdd;
                var valueGenerated = property.ValueGenerated;
                var hasDefaultValue = string.IsNullOrWhiteSpace(column.DefaultValueSql) == false ||
                 string.IsNullOrWhiteSpace(column.ComputedColumnSql) == false ||
                    valueGenerated == ValueGenerated.OnAddOrUpdate;
                return new ParameterModel()
                {
                    Name = column.Name,
                    Type = column.StoreType,
                    IsPrimaryKey = isPrimaryKey,
                    Precision = column.Precision,
                    Scale = column.Scale,
                    IsUnicode = column.IsUnicode,
                    IsFixedLength = column.IsFixedLength,
                    IsAutoIncrement = isIdentity,
                    HasDefaultValue = hasDefaultValue
                };
            }).ToList();
        }
        protected string GetEntityName()
        {
            var tableName = _table.EntityTypeMappings.First().EntityType.ClrType.Name;
            return tableName;
        }

    }
}