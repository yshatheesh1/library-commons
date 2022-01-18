using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class ClassModel
    {
        public string Name { get; set; }
        public List<PropertyModel> Properties { get; set; }

        public List<ClassModel> NestedClass { get; set; }

        public ClassModel()
        {
            Properties = new List<PropertyModel>();
            NestedClass = new List<ClassModel>();
        }
    }

    public class PropertyModel
    {
        public string Name { get; set; }
        public bool IsNullable { get; set; }
        public bool IsValueType {get;set;}
        public bool IsList { get; set; }
        public Type CSharpType { get; set; }
    }
}