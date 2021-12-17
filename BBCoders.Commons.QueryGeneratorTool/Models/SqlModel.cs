using System.Collections.Generic;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class SqlModel
    {
        public string MethodName { get; set; }
        public string Sql { get; set; }
        public List<SqlProjection> Projections { get; set; }
        public List<SqlBinding> EqualBindings { get; set; }
        public List<SqlBinding> InBindings { get; set; }

        public SqlModel(string name) {
            this.MethodName = name;
            Projections = new List<SqlProjection>();
            EqualBindings = new List<SqlBinding>();
            InBindings = new List<SqlBinding>();
        }
    }
}