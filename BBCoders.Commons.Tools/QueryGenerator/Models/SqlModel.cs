using System.Collections.Generic;

namespace BBCoders.Commons.Tools.QueryGenerator.Models
{
     public class SqlModel
    {
        public string MethodName { get; set; }
        public string Sql { get; set; }
        public List<SqlProjection> Projections { get; set; }
        public List<SqlBinding> Bindings { get; set; }
    }
}