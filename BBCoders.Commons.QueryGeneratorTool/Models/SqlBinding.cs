using System;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class SqlBinding
    {
        /// <summary>
        /// Name of the parameter 
        /// ex: @__Value_1, @__Value_2
        /// if list ex: @__inValue_1, @__inValue_2 etc
        /// </summary>
        /// <value>get or set name of the parameter</value>
        public string Name { get; set; }
        /// <summary>
        /// Value to use for binding
        /// ex: firstname, lastname
        /// </summary>
        /// <value>get or set value to use</value>
        public InputParameter Value { get; set; }
        /// <summary>
        /// default value for binding
        /// </summary>
        /// <value>get or set default value</value>
        public object DefaultValue { get; set; }
    }

}