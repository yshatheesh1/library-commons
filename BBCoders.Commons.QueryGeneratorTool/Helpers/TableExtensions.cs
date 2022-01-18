using System.Collections.Generic;
using System.Linq;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Helpers
{
    public static class TableExtensions
    {
        public static List<ParameterModel> GetMappings(this ITable table)
        {
            return table.Columns.Select(column =>
            {
                var property = column.PropertyMappings.First().Property;
                var primaryKey = table.PrimaryKey.Columns.FirstOrDefault(x => x.Name.Equals(column.Name));
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

    }
}