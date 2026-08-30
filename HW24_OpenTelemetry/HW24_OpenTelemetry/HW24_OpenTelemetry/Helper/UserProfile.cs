using HW24_OpenTelemetry.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW24_OpenTelemetry.Helper
{
    [GenerateBinarySerializer]
    public partial class UserProfile
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
