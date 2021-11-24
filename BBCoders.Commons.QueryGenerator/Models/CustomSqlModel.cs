using System.Collections.Generic;

namespace BBCoders.Commons.QueryGenerator.Models
{
     public class CustomSqlModel
    {
        public string MethodName { get; set; }
        public string Sql { get; set; }
        public List<SqlProjection> Projections { get; set; }
        public List<SqlBinding> Bindings { get; set; }
    }
}