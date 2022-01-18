using System;
using System.Collections.Generic;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class SqlModel
    {
        /// <summary>
        /// Name of the method
        /// </summary>
        /// <value></value>
        public string MethodName { get; set; }
        /// <summary>
        /// Sql for the method
        /// </summary>
        /// <value></value>
        public string Sql { get; set; }
        /// <summary>
        /// output from database
        /// </summary>
        /// <value></value>
        public List<SqlProjection> Projections { get; set; }
        /// <summary>
        /// Inputs provided for the querying database
        /// </summary>
        /// <value></value>
        public List<InputParameter> Parameters { get; set; }
        /// <summary>
        /// binding parameters for query
        /// </summary>
        /// <value></value>
        public List<SqlBinding> BindingParameters { get; set; }

        public SqlModel(string name)
        {
            this.MethodName = name;
            Projections = new List<SqlProjection>();
            BindingParameters = new List<SqlBinding>();
            Parameters = new List<InputParameter>();
        }
    }

    public class InputParameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsNullable { get; set; }
        public bool IsList { get; set; }
        public string ListPlaceholder { get; set; }
    }
}