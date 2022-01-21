using System.Collections.Generic;
using Humanizer;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class MethodOperation
    {
        /// <summary>
        /// method name
        /// </summary>
        /// <value></value>
        public string MethodName { get; set; }
        /// <summary>
        ///  type of sql statement
        /// </summary>
        /// <value></value>
        public SqlType SqlType { get; set; }
        /// <summary>
        /// input model name
        /// </summary>
        /// <value></value>
        public string InputModelName { get; set; }
        /// <summary>
        /// is batch operation
        /// </summary>
        /// <value></value>
        public bool IsBatchOperation { get; set; }
        /// <summary>
        /// input model parameters
        /// </summary>
        /// <value></value>
        public List<ModelParameter> InputModel { get; set; }
        public bool HasResult { get; set; }
        public string OutputModelName { get; set; }
        /// <summary>
        /// contains only output model properties
        /// </summary>
        /// <value></value>
        public List<ModelParameter> OutputModel { get; set; }
        /// <summary>
        /// contains all model properties
        /// </summary>
        public List<ModelParameter> Table;
        public string CustomSql { get; set; }

        public MethodOperation()
        {
            InputModel = new List<ModelParameter>();
            OutputModel = new List<ModelParameter>();
        }
    }

    public class ModelParameter
    {
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsNullable { get; set; }
        public string Type { get; set; }
        public object DefaultValue { get; set; }
        public bool IsListType { get; set; }
        public bool IsvalueType { get; set; }
    }

    public enum SqlType
    {
        Select,
        Insert,
        Update,
        Delete,
        Custom
    }
}