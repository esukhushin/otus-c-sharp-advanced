using HW24_OpenTelemetry;
using HW24_OpenTelemetry.Helper;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

internal class Program
{
    private const string _ip = "127.0.0.1";
    private const int _port = 8081;

    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("HW24_OpenTelemetry")
            .AddConsoleExporter()
            .Build();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("HW24_OpenTelemetry")
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
            await tcpServer.StartAsync(_ip, _port, activity?.Context, cts.Token);
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