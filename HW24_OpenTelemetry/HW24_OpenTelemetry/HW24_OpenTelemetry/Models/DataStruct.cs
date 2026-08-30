using System;
using System.Collections.Generic;
using System.Text;

namespace HW24_OpenTelemetry.Models
{
    public ref struct DataStruct<T>
    {
        public ReadOnlySpan<T> Command { get; set; }
        public ReadOnlySpan<T> Key { get; set; }
        public ReadOnlySpan<T> Value { get; set; }
    }
}
