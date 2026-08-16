using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Roslyn.Models
{
    public class PropertyInfo
    {
        public string Name { get; }
        public SpecialType SpecialType { get; }
        public string Type { get; }
        public int? Size { get; }
        
        public PropertyInfo(string name, SpecialType specialType,  string type, int? size)
        {
            Name = name;
            SpecialType = specialType;
            Type = type;
            Size = size;
        }
    }
}
