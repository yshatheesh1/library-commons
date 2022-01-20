using System;
using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Helpers;
using BBCoders.Commons.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.QueryGeneratorTool.Services.SqlGenerator
{
    public class SQLGenerator : ISQLGenerator
    {
        public ISqlGenerationHelper _sqlGenerationHelper { get; set; }
        public SQLGenerator(ISqlGenerationHelper sqlGenerationHelper)
        {
            this._sqlGenerationHelper = sqlGenerationHelper;
        }

        public void Select(IndentedStringBuilder builder, ITable table, string[] whereMappings, Boolean isDynamic = false)
        {
            var parameters = table.GetMappings();
            var keyColumns = parameters.Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            if (isDynamic)
            {
                builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} {WhereInClause(table.Name, columns, whereMappings, true)};");
            }
            else
            {
                builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} {WhereEqualClause(table.Name, columns, whereMappings, true)};");
            }
        }


        public void Delete(IndentedStringBuilder builder, ITable table, string[] whereMappings, bool isDynamic = false)
        {
            var keyColumns = table.GetMappings().Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            if (isDynamic)
            {
                builder.Append($"DELETE FROM {DelimitTable(table.Name, table.Schema, true)} {WhereInClause(table.Name, columns, whereMappings, true)};");
            }
            else
            {
                builder.Append($"DELETE FROM {DelimitTable(table.Name, table.Schema, true)} {WhereEqualClause(table.Name, columns, whereMappings, true)};");
            }
        }

        public void Insert(IndentedStringBuilder builder, ITable table, string[] insertValues)
        {
            var columnMappings = table.GetMappings();
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            var columns = columnMappings.Where(x => !x.isAutoIncrement());
            var insertColumns = columns.Select(column => DelimitColumn(table.Name, column.Name, false));

            builder.Append($"INSERT INTO {DelimitTable(table.Name, table.Schema, false)} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
        }

        public void SelectLastInserted(IndentedStringBuilder builder, ITable table)
        {
            var parameters = table.GetMappings();
            var columns = parameters.Where(x => x.isPrimaryKey()).Select(x => x.Name).ToArray();
            var autoIncrementColumn = parameters.FirstOrDefault(x => x.isAutoIncrement());
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} WHERE {DelimitColumn(table.Name, autoIncrementColumn.Name, true)} = LAST_INSERT_ID() AND ROW_COUNT() = 1;");
        }


        public void Update(IndentedStringBuilder builder, ITable table, string[] setMappings, string[] whereMappings)
        {
            var parameters = table.GetMappings();
            var columns = parameters.Where(x => !x.isPrimaryKey()).Select(x => x.Name).ToArray();
            var keyColumns = parameters.Where(x => x.isPrimaryKey()).Select(x => x.Name).ToArray();
           // var keyColumnMappings = keyColumns.Select(column => "@" + column + placeholder).ToArray();
            // var setColumn = new string[columns.Count()];
            // var setValue = new string[columns.Count()];
            // for (var i = 0; i < columns.Count(); i++)
            // {
            //     var column = columns.ElementAt(i);
            //     var delimitColumn = DelimitColumn(table.Name, column.Name, true);
            //     setColumn[i] = column.Name;
            //     setValue[i] = column.hasDefaultValue() ? $"If(@{column.Name}{placeholder} IS NULL,DEFAULT({delimitColumn}), @{column.Name}{placeholder})" : $"@{column.Name}{placeholder}";
            // }
            /*
                UPDATE `States` SET `Name` = @p0, `StateId` = @p1 WHERE `Id` = @p2;
                UPDATE `States` SET `Name` = @p3, `StateId` = @p4 WHERE `Id` = @p5;
            */
            builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, columns, setMappings, true)} {WhereEqualClause(table.Name, keyColumns, whereMappings, true)};");

            //  builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, setColumn, setValue, true)} {WhereClause(table.Name, keyColumns, keyColumnMappings, true)};");
        }



        public void Insert(IndentedStringBuilder builder, ITable table)
        {
            var columnMappings = table.GetMappings();
            var autoIncrementColumn = columnMappings.FirstOrDefault(x => x.isAutoIncrement());
            var columns = columnMappings.Where(x => !x.isAutoIncrement());
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            var insertColumns = new string[columns.Count()];
            var insertValues = new string[columns.Count()];
            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns.ElementAt(i);
                insertColumns[i] = DelimitColumn(table.Name, column.Name, false);
                insertValues[i] = column.hasDefaultValue() ? $"If(@{column.Name} IS NULL,DEFAULT({DelimitColumn(table.Name, column.Name)}), @{column.Name})" : $"@{column.Name}";
            }
            builder.AppendLine($"INSERT INTO {table.Name} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
            if (autoIncrementColumn != null)
                builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} WHERE {DelimitColumn(table.Name, autoIncrementColumn.Name, true)} = LAST_INSERT_ID();");
            else
            {
                var keyColumns = table.PrimaryKey.Columns.Select(x => "@" + x.Name).ToArray();
                Select(builder, table, keyColumns, false);
            }

        }

        public void Delete(IndentedStringBuilder builder, ITable table)
        {
            var keyColumns = table.GetMappings().Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            var columnMappings = columns.Select(x => "@" + x).ToArray();
            builder.Append($"DELETE FROM {DelimitTable(table.Name, table.Schema, true)} {WhereEqualClause(table.Name, columns, columnMappings, true)}");
        }

        public void Update(IndentedStringBuilder builder, ITable table)
        {
            var parameters = table.GetMappings();
            var columns = parameters.Where(x => !x.isPrimaryKey());
            var keyColumns = parameters.Where(x => x.isPrimaryKey()).Select(x => x.Name).ToArray();
            var keyColumnMappings = keyColumns.Select(x => "@" + x).ToArray();
            var setColumn = new string[columns.Count()];
            var setValue = new string[columns.Count()];
            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns.ElementAt(i);
                var delimitColumn = DelimitColumn(table.Name, column.Name, true);
                setColumn[i] = column.Name;
                setValue[i] = column.hasDefaultValue() ? $"If(@{column.Name} IS NULL,DEFAULT({delimitColumn}), @{column.Name})" : $"@{column.Name}";
            }
            builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, setColumn, setValue, true)} {WhereEqualClause(table.Name, keyColumns, keyColumnMappings, true)};");
            Select(builder, table, keyColumnMappings, false);
        }

        protected string WhereInClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return InClause(table, columns, columnMappings, alias, "WHERE ", " AND ");
        }

        protected string WhereEqualClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return EqualClause(table, columns, columnMappings, alias, "WHERE ", " AND ");
        }

        protected string SetClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return EqualClause(table, columns, columnMappings, alias, "SET ", ", ");
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
            return _sqlGenerationHelper.DelimitIdentifier(tableName) + "." + _sqlGenerationHelper.DelimitIdentifier(column);
        }

        private string InClause(string table, string[] columns, string[] columnMappings, bool alias, string clause, string delimiter)
        {
            var delimitedColumns = columns.Select((x, idx) => DelimitColumn(table, x, alias) + $" IN (\" + {columnMappings[idx]} + \")");
            return clause + string.Join(delimiter, delimitedColumns);
        }

        private string EqualClause(string table, string[] columns, string[] columnMappings, bool alias, string clause, string delimiter)
        {
            var delimitedColumns = columns.Select((x, idx) => DelimitColumn(table, x, alias) + " = " + columnMappings[idx]);
            return clause + string.Join(delimiter, delimitedColumns);
        }
    }
}