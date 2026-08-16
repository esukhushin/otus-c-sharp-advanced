using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Roslyn.Models
{
    public class PropertyInfo
    {
        public string Name { get; set; }
        public SpecialType SpecialType { get; set; }
        public string Type { get; set; }
        public int? Size { get; set; }
        
        public PropertyInfo(string name, SpecialType specialType,  string type, int? size)
        {
            Name = name;
            SpecialType = specialType;
            Type = type;
            Size = size;
        }
    }
}
