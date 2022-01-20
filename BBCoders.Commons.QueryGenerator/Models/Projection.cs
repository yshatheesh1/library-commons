using System;

namespace  BBCoders.Commons.QueryGenerator
{
    /// <summary>
    /// Projection for select query
    /// </summary>
    public class Projection
    {
        /// <summary>
        /// Name of the projection
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Which table the projection refers to
        /// </summary>
        /// <value></value>
        public string Table { get; set; }

        /// <summary>
        /// type of the projection
        /// </summary>
        /// <value></value>
        public Type Type { get; set; }
        /// <summary>
        /// Check if type is nullable
        /// </summary>
        /// <value></value>
        public bool IsNullable { get; set; }
    }

}