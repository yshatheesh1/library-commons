using System;

namespace BBCoders.Commons.Tools.QueryGenerator.Models
{
    public class SqlProjection
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsNullable { get; set; }
    }

}