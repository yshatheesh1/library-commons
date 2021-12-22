using System;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class SqlBinding
    {
        /// <summary>
        /// Name of the parameter 
        /// ex: @__Value_1, @__Value_2
        /// </summary>
        /// <value>get or set name of the parameter</value>
        public string Name { get; set; }
        /// <summary>
        /// Type of the parameter
        /// ex: Int32, Boolean etc
        /// </summary>
        /// <value>get or set type of parameter</value>
        public string Type { get; set; }
        /// <summary>
        /// Value to use for binding
        /// ex: firstname, lastname
        /// </summary>
        /// <value>get or set value to use</value>
        public string Value { get; set; }
        /// <summary>
        /// Checks if binding value has default value
        /// </summary>
        /// <value>get or set default value</value>
        public bool hasDefault { get; set; }
        /// <summary>
        /// default value for binding
        /// </summary>
        /// <value>get or set default value</value>
        public object DefaultValue { get; set; }

        public string DbType { get; set; }
    }

}