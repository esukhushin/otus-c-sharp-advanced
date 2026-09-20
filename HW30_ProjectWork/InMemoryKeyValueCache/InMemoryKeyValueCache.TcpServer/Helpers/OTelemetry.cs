using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace InMemoryKeyValueCache.TcpServer.Helpers
{
    public static class OTelemetry
    {
        public static readonly ActivitySource ActivitySource = new("InMemoryKeyValueCache");
        public static readonly Meter Meter = new("InMemoryKeyValueCache");
        public static readonly Counter<long> Counter = Meter.CreateCounter<long>(
            "InMemoryKeyValueCache.ProcessClientAsync.Count",
            description: "Total requests");
        public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>(
            "InMemoryKeyValueCache.ProcessClientAsync.Duration",
            unit: "ms",
            description: "Request Duration");
    }
}
