using HW06_IDisposableAsync;

internal class Program
{
    private const string _ip = "127.0.0.1";
    private const int _port = 8080;

    private static async Task Main(string[] args)
    {
        await new TcpServer().StartAsync(_ip, _port);

        Console.ReadKey();
    }
}