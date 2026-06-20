using HW06_IDisposableAsync;
using System.Text;

internal class Program
{
    private const string _ip = "127.0.0.1";
    private const int _port = 8080;

    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Завершение сервера");
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