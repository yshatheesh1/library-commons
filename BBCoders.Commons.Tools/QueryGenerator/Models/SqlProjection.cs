using System;

namespace BBCoders.Commons.Tools.QueryGenerator.Models
{
    public class SqlProjection
    {
        /// <summary>
        /// Name of the projection
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// type of the projection
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
        /// <summary>
        /// Check if type is nullable
        /// </summary>
        /// <value></value>
        public bool IsNullable { get; set; }
        /// <summary>
        /// Check if type is value or reference type
        /// </summary>
        /// <value></value>
        public bool IsValueType { get; set; }
    }

}