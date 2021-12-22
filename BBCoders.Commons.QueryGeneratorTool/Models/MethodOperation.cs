using System.Collections.Generic;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class MethodOperation
    {
        public string MethodName { get; set; }
        public string Sql { get; set; }
        public string InputModel { get; set; }
        public List<ModelParameter> InputModelParameters { get; set; }
        public bool HasResult { get; set; }
        public bool UpdateInputModel { get; set; }
        public string OutputModel { get; set; }
        public List<ModelParameter> OutputModelParameters { get; set; }
    }

    public class ModelParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string DbType { get; set; }
        public string Type { get; set; }

    }
}