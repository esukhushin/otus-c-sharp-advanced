using InMemoryKeyValueCache.Common.Helpers;
using InMemoryKeyValueCache.TcpServer;
using InMemoryKeyValueCache.TcpServer.Helpers;
using System.Diagnostics;
using System.Text;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("InMemoryKeyValueCache")
            .AddConsoleExporter()
            .Build();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("InMemoryKeyValueCache")
            .AddConsoleExporter()
            .Build();

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Завершение сервера");
            e.Cancel = true;
            cts.Cancel();
        };

        using var simpleStore = new SimpleStore();
        using var tcpServer = new TcpServer(simpleStore);

        Activity? activity = null;
        try
        {
            activity = OTelemetry.ActivitySource.StartActivity(nameof(Main), ActivityKind.Internal);
            await tcpServer.StartAsync(ConfigData.IP, ConfigData.Port, activity?.Context, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        finally
        {
            activity?.Dispose();
        }

        Console.ReadKey();
    }
}