using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BBCoders.Commons.QueryConfiguration;
using BBCoders.Commons.Tools.QueryGenerator.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public abstract class SqlOperationGenerator : ISqlOperationGenerator
    {
        protected ISqlGenerationHelper _sqlGenerationHelper;
        protected ITable _table;
        public SqlOperationGenerator(ISqlGenerationHelper sqlGenerationHelper, ITable procedureOperation)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
            this._table = procedureOperation;
        }

        public abstract void GenerateSql(IndentedStringBuilder migrationCommandListBuilder);
        public abstract void GenerateModel(IndentedStringBuilder builder);
        public abstract void GenerateMethod(IndentedStringBuilder builder, string connectionString);

        protected void GenerateModel(IndentedStringBuilder builder, string className, IEnumerable<IProperty> properties)
        {
            builder.AppendLine($"public class {className}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var property in properties)
                {
                    builder.Append($"public {getTypeName(property.ClrType)} {property.Name}")
                            .AppendLine(" { get; set; }");
                }
            }
            builder.AppendLine("}");
        }

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

        protected string getTypeName(Type clrType)
        {
            var nullableType = Nullable.GetUnderlyingType(clrType);
            return nullableType != null ? nullableType.Name + "?" : clrType.Name;
        }

        protected string PascalCase(string name)
        {
            var result = new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();
            return new CultureInfo("en-US", false).TextInfo.ToTitleCase(result);
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