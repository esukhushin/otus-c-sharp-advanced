using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace HW24_OpenTelemetry.Helper
{
    public static class OTelemetry
    {
        public static readonly ActivitySource ActivitySource = new("HW24_OpenTelemetry");
        public static readonly Meter Meter = new("HW24_OpenTelemetry");
        public static readonly Counter<long> Counter = Meter.CreateCounter<long>(
            "HW24_OpenTelemetry.ProcessClientAsync.Count",
            description: "Общее количество запросов");
        public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>(
            "HW24_OpenTelemetry.ProcessClientAsync.Duration",
            unit: "ms",
            description: "Время запроса");
    }
}
