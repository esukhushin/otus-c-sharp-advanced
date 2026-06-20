using HW06_IDisposableAsync;

internal class Program
{
    private const string _ip = "127.0.0.1";
    private const int _port = 8080;

    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Завешение сервера");
            e.Cancel = true;
            cts.Cancel();
        };

        using var tcpServer = new TcpServer();

        try
        {
            await tcpServer.StartAsync(_ip, _port, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Сервер завершен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}