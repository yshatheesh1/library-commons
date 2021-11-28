using System.Collections.Generic;

namespace BBCoders.Commons.Tools.QueryGenerator.Models
{
    public class ParameterModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string StoreType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool? IsFixedLength { get; set; }
        public bool? IsUnicode { get; set; }
        public bool? IsPrimaryKey { get; set; }
        public bool? IsAutoIncrement { get; set; }
        public bool? HasDefaultValue { get; set; }

        public bool isPrimaryKey()
        {
            return IsPrimaryKey.HasValue && IsPrimaryKey.Value;
        }

        public bool hasDefaultValue()
        {
            return HasDefaultValue.HasValue && HasDefaultValue.Value;
        }

        public bool isAutoIncrement()
        {
            return IsAutoIncrement.HasValue && IsAutoIncrement.Value;
        }
    }

}