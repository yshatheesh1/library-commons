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

        public void SelectBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings)
        {
            var parameters = table.GetMappings();
            var columns = parameters.Where(x => x.isPrimaryKey()).Select(x => x.Name).ToArray();
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            if (columns.Length != whereMappings.Length)
            {
                Reporter.WriteError("Where Columns and Values doesn't match for the query");
                throw new Exception("Where Columns and Values doesn't match for the query");
            }
            /*
                  SELECT * FROM `States` AS `s` WHERE `s`.`Id` In (@Id1,@Id2);
            */
            builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} {WhereInClause(table.Name, columns, whereMappings, true)};");
        }

        public void InsertBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings, string placeholder)
        {
            var columnMappings = table.GetMappings();
            var autoIncrementColumn = columnMappings.FirstOrDefault(x => x.isAutoIncrement());
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            var columns = columnMappings.Where(x => !x.isAutoIncrement());
            var insertColumns = columns.Select(column => DelimitColumn(table.Name, column.Name, false));
            var insertValues = columns.Select(column => "@" + column.Name + "{" + placeholder + "}");
            /*
                INSERT INTO `States` (`Name`, `StateId`) VALUES (@p10, @p11);
                SELECT `Id` FROM `States` WHERE `Id`= LAST_INSERT_ID();

                INSERT INTO `States` (`Name`, `StateId`) VALUES (@p12, @p13);
                SELECT `Id` FROM `States` WHERE `Id`= LAST_INSERT_ID();
            */
            builder.Append("$\"");
            builder.Append($"INSERT INTO {DelimitTable(table.Name, table.Schema, false)} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
            if (autoIncrementColumn != null)
                builder.Append($" SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} WHERE {DelimitColumn(table.Name, autoIncrementColumn.Name, true)} = LAST_INSERT_ID();");
            else
                SelectBatch(builder, table, whereMappings);

        }

        public void DeleteBatch(IndentedStringBuilder builder, ITable table, string[] columnMappings)
        {
            var keyColumns = table.GetMappings().Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            /*
            DELETE FROM `States` WHERE `Id` in (@p0, @p1);
            */
            builder.Append($"DELETE FROM {DelimitTable(table.Name, table.Schema, true)} {WhereInClause(table.Name, columns, columnMappings, true)}");
        }

        public void UpdateBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings, string placeholder)
        {
            var parameters = table.GetMappings();
            var columns = parameters.Where(x => !x.isPrimaryKey()).ToArray();
            var keyColumns = parameters.Where(x => x.isPrimaryKey()).Select(x => x.Name).ToArray();
            var setColumn = new string[columns.Count()];
            var setValue = new string[columns.Count()];
            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns.ElementAt(i);
                var delimitColumn = DelimitColumn(table.Name, column.Name, true);
                setColumn[i] = column.Name;
                setValue[i] = column.hasDefaultValue() ? $"If(@{column.Name}{placeholder} IS NULL,DEFAULT({delimitColumn}), @{column.Name}{placeholder})" : $"@{column.Name}{placeholder}";
            }
            /*
                UPDATE `States` SET `Name` = @p0, `StateId` = @p1 WHERE `Id` = @p2;
                UPDATE `States` SET `Name` = @p3, `StateId` = @p4 WHERE `Id` = @p5;
            */
            builder.Append("$\"");
            builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, setColumn, setValue, true, placeholder)} {WhereEqualClause(table.Name, keyColumns, whereMappings, true)};");
            Select(builder, table, placeholder);

            //  builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, setColumn, setValue, true)} {WhereClause(table.Name, keyColumns, keyColumnMappings, true)};");
        }

        public void Select(IndentedStringBuilder builder, ITable table, string placeholder = "")
        {
            var parameters = table.GetMappings();
            var keyColumns = parameters.Where(x => x.isPrimaryKey());
            var columns = keyColumns.Select(x => x.Name).ToArray();
            var selectColumns = table.Columns.Select(column => DelimitColumn(table.Name, column.Name, true));
            var columnMappings = columns.Select(x => "@" + x + placeholder).ToArray();
            builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} {WhereEqualClause(table.Name, columns, columnMappings, true)};");
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
                Select(builder, table);
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
            builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, setColumn, setValue, true, "")} {WhereEqualClause(table.Name, keyColumns, keyColumnMappings, true)};");
            Select(builder, table);
        }

        protected string WhereInClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return InClause(table, columns, columnMappings, alias, "WHERE ", "AND ");
        }

        protected string WhereEqualClause(string table, string[] columns, string[] columnMappings, bool alias)
        {
            return EqualClause(table, columns, columnMappings, alias, "WHERE ", "AND ", "");
        }

        protected string SetClause(string table, string[] columns, string[] columnMappings, bool alias, string placeholder)
        {
            return EqualClause(table, columns, columnMappings, alias, "SET ", ", ", "");
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
            var delimitedColumns = columns.Select((x, idx) => DelimitColumn(table, x, alias) + $" IN (\" + {columnMappings[idx]} + @\")");
            return clause + string.Join(delimiter, delimitedColumns);
        }

        private string EqualClause(string table, string[] columns, string[] columnMappings, bool alias, string clause, string delimiter, string placeholder)
        {
            var delimitedColumns = columns.Select((x, idx) => DelimitColumn(table, x, alias) + " = " + columnMappings[idx] + placeholder);
            return clause + string.Join(delimiter, delimitedColumns);
        }
    }
}