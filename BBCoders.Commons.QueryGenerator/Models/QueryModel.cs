using System;
using System.Collections.Generic;

namespace  BBCoders.Commons.QueryGenerator
{
    /// <summary>
    /// custom query model
    /// </summary>
    public class QueryModel
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
        public List<Projection> Projections { get; set; }
        /// <summary>
        /// Inputs provided for the querying database
        /// </summary>
        /// <value></value>
        public List<Parameter> Parameters { get; set; }
        /// <summary>
        /// binding parameters for query
        /// </summary>
        /// <value></value>
        public List<Binding> Bindings { get; set; }

        /// <summary>
        /// Constructor for query model
        /// </summary>
        /// <param name="name"></param>
        public QueryModel(string name)
        {
            this.MethodName = name;
            Projections = new List<Projection>();
            Bindings = new List<Binding>();
            Parameters = new List<Parameter>();
        }
    }
}