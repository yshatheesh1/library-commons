namespace BBCoders.Commons.Tools.QueryGenerator.Models
{
    public class SqlBinding
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool hasDefault { get; set; }
        public object DefaultValue { get; set; }
    }

}