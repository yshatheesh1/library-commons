using System;
using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Helpers;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
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

        public void Select(IndentedStringBuilder builder, List<ModelParameter> table, string[] whereMappings, Boolean isDynamic = false)
        {
            var tableName = table.First().TableName;
            var tableSchema = table.First().SchemaName;
            var keyColumns = table.Where(x => x.IsPrimaryKey);
            var columns = keyColumns.Select(x => x.ColumnName).ToArray();
            var selectColumns = table.Select(column => DelimitColumn(tableName, column.ColumnName, true));
            if (isDynamic)
            {
                builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(tableName, tableSchema, true)} {WhereInClause(tableName, columns, whereMappings, true)};");
            }
            else
            {
                builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(tableName, tableSchema, true)} {WhereEqualClause(tableName, columns, whereMappings, true)};");
            }
        }

        public void Delete(IndentedStringBuilder builder, List<ModelParameter> table, string[] whereMappings, bool isDynamic = false)
        {
            var tableName = table.First().TableName;
            var tableSchema = table.First().SchemaName;
            var keyColumns = table.Where(x => x.IsPrimaryKey);
            var columns = keyColumns.Select(x => x.ColumnName).ToArray();
            if (isDynamic)
            {
                builder.Append($"DELETE FROM {DelimitTable(tableName, tableSchema, true)} {WhereInClause(tableName, columns, whereMappings, true)};");
            }
            else
            {
                builder.Append($"DELETE FROM {DelimitTable(tableName, tableSchema, true)} {WhereEqualClause(tableName, columns, whereMappings, true)};");
            }
        }

        public void Insert(IndentedStringBuilder builder, List<ModelParameter> table, string[] insertValues)
        {
            var tableName = table.First().TableName;
            var tableSchema = table.First().SchemaName;
            var selectColumns = table.Select(column => DelimitColumn(tableName, column.ColumnName, true));
            var columns = table.Where(x => !x.IsAutoIncrement);
            var insertColumns = columns.Select(column => DelimitColumn(tableName, column.ColumnName, false));

            builder.Append($"INSERT INTO {DelimitTable(tableName, tableSchema, false)} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
        }

        public void SelectLastInserted(IndentedStringBuilder builder, List<ModelParameter> table)
        {
            var tableName = table.First().TableName;
            var tableSchema = table.First().SchemaName;
            var columns = table.Where(x => x.IsPrimaryKey).Select(x => x.ColumnName).ToArray();
            var autoIncrementColumn = table.FirstOrDefault(x => x.IsAutoIncrement);
            var selectColumns = table.Select(column => DelimitColumn(tableName, column.ColumnName, true));
            builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(tableName, tableSchema, true)} WHERE {DelimitColumn(tableName, autoIncrementColumn.ColumnName, true)} = LAST_INSERT_ID() AND ROW_COUNT() = 1;");
        }


        public void Update(IndentedStringBuilder builder, List<ModelParameter> table, string[] setMappings, string[] whereMappings)
        {
            var tableName = table.First().TableName;
            var tableSchema = table.First().SchemaName;
            var columns = table.Where(x => !x.IsPrimaryKey).Select(x => x.ColumnName).ToArray();
            var keyColumns = table.Where(x => x.IsPrimaryKey).Select(x => x.ColumnName).ToArray();
            builder.Append($"UPDATE {DelimitTable(tableName, tableSchema, true)} {SetClause(tableName, columns, setMappings, true)} {WhereEqualClause(tableName, keyColumns, whereMappings, true)};");
        }



        // public void Insert(IndentedStringBuilder builder, List<ModelParameter> table)
        // {
        //     var tableName = table.First().TableName;
        //     var tableSchema = table.First().SchemaName;
        //     var autoIncrementColumn = table.FirstOrDefault(x => x.IsAutoIncrement);
        //     var columns = table.Where(x => !x.IsAutoIncrement);
        //     var selectColumns = table.Select(column => DelimitColumn(tableName, column.ColumnName, true));
        //     var insertColumns = new string[columns.Count()];
        //     var insertValues = new string[columns.Count()];
        //     for (var i = 0; i < columns.Count(); i++)
        //     {
        //         var column = columns.ElementAt(i);
        //         insertColumns[i] = DelimitColumn(table.Name, column.Name, false);
        //         insertValues[i] = column.hasDefaultValue() ? $"If(@{column.Name} IS NULL,DEFAULT({DelimitColumn(table.Name, column.Name)}), @{column.Name})" : $"@{column.Name}";
        //     }
        //     builder.AppendLine($"INSERT INTO {table.Name} ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)});");
        //     if (autoIncrementColumn != null)
        //         builder.Append($"SELECT {string.Join(",", selectColumns)} FROM {DelimitTable(table.Name, table.Schema, true)} WHERE {DelimitColumn(table.Name, autoIncrementColumn.Name, true)} = LAST_INSERT_ID();");
        //     else
        //     {
        //         var keyColumns = table.PrimaryKey.Columns.Select(x => "@" + x.Name).ToArray();
        //         Select(builder, table, keyColumns, false);
        //     }

        // }

        public void Delete(IndentedStringBuilder builder, List<ModelParameter> table)
        {
            var tableName = table.First().TableName;
            var tableSchema = table.First().SchemaName;
            var keyColumns = table.Where(x => x.IsPrimaryKey);
            var columns = keyColumns.Select(x => x.ColumnName).ToArray();
            var columnMappings = columns.Select(x => "@" + x).ToArray();
            builder.Append($"DELETE FROM {DelimitTable(tableName, tableSchema, true)} {WhereEqualClause(tableName, columns, columnMappings, true)}");
        }

        // public void Update(IndentedStringBuilder builder, List<ModelParameter> table)
        // {
        //     var tableName = table.First().TableName;
        //     var tableSchema = table.First().SchemaName;
        //     var columns = table.Where(x => !x.IsPrimaryKey);
        //     var keyColumns = table.Where(x => x.IsPrimaryKey).Select(x => x.ColumnName).ToArray();
        //     var keyColumnMappings = keyColumns.Select(x => "@" + x).ToArray();
        //     var setColumn = new string[columns.Count()];
        //     var setValue = new string[columns.Count()];
        //     for (var i = 0; i < columns.Count(); i++)
        //     {
        //         var column = columns.ElementAt(i);
        //         var delimitColumn = DelimitColumn(tableName, column.ColumnName, true);
        //         setColumn[i] = column.ColumnName;
        //         setValue[i] = column.hasDefaultValue() ? $"If(@{column.Name} IS NULL,DEFAULT({delimitColumn}), @{column.Name})" : $"@{column.Name}";
        //     }
        //     builder.Append($"UPDATE {DelimitTable(table.Name, table.Schema, true)} {SetClause(table.Name, setColumn, setValue, true)} {WhereEqualClause(table.Name, keyColumns, keyColumnMappings, true)};");
        //     Select(builder, table, keyColumnMappings, false);
        // }

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