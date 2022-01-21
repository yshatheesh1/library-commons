using System.Collections.Generic;
using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using Humanizer;
using BBCoders.Commons.QueryGenerator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.QueryGeneratorTool.Helpers
{
    public static class TableExtensions
    {
        public static List<ModelParameter> GetMappings(this ITable table, Language language, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            return table.Columns.Select(column => column.CreateModelParameter(table, language, relationalTypeMappingSource)).ToList();
        }

        public static Type GetDdbType(this IProperty property, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            // sometimes database may have different property
            var clrType = property.ClrType;
            var relationalTypeMapping = relationalTypeMappingSource.FindMapping(property);
            if (relationalTypeMapping.DbType.HasValue)
            {
                clrType = SqlMapperHelper.getClrType(relationalTypeMapping.DbType.Value);
            }
            return clrType;
        }

        public static string GetDbTypeName(this IProperty property, Language language, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            // sometimes database may have different property
            var clrType = property.GetDdbType(relationalTypeMappingSource);
            var type = language.Type[clrType];
            if (type == null)
            {
                throw new Exception("Type not found to create model - " + clrType);
            }
            return type;
        }

        public static string GetLanguageType(this Type clrType, Language language)
        {
            // sometimes database may have different property
            var type = language.Type[clrType];
            if (type == null)
            {
                throw new Exception("Type not found to create model - " + clrType);
            }
            return type;
        }
        public static ModelParameter CreateModelParameter(this IColumn column, ITable table, Language language, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            return new ModelParameter()
            {
                ColumnName = column.Name,
                TableName = table.Name,
                SchemaName = table.Schema,
                PropertyName = column.PropertyMappings.First().Property.Name,
                Type = column.PropertyMappings.First().Property.GetDbTypeName(language, relationalTypeMappingSource),
                IsNullable = column.PropertyMappings.First().Property.IsNullable,
                IsPrimaryKey = table.PrimaryKey.Columns.Any(x => x.Name.Equals(column.Name)),
                IsAutoIncrement = table.PrimaryKey.Columns.Any(x => x.Name.Equals(column.Name) && x.PropertyMappings.First().Property.ValueGenerated == ValueGenerated.OnAdd)
            };
        }

        public static string GetJoinPlaceholder(this string name)
        {
            return name.Pluralize() + "Joined";
        }

    }
}