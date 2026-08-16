using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Roslyn.Models
{
    public class ClassInfo
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public List<PropertyInfo> Properties { get; set; }

        public ClassInfo(string className, string @namespace, List<PropertyInfo> properties)
        {
            ClassName = className;
            Namespace = @namespace;
            Properties = properties;
        }
    }
}
