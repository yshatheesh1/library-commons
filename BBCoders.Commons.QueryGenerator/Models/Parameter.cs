using System;

namespace  BBCoders.Commons.QueryGenerator
{

    /// <summary>
    /// Input parameter
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// name of the parameter
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// type of the parameter
        /// </summary>
        /// <value></value>
        public Type Type { get; set; }
        /// <summary>
        /// is input nullable
        /// </summary>
        /// <value></value>
        public bool IsNullable { get; set; }
        /// <summary>
        /// is list type
        /// </summary>
        /// <value></value>
        public bool IsList { get; set; }
    }
}