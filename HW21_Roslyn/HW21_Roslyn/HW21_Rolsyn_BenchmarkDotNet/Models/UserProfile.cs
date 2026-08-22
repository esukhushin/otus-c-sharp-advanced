using HW21_Rolsyn_BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Rolsyn_BenchmarkDotNet.Models
{
    [GenerateBinarySerializer]
    public partial class UserProfile
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
