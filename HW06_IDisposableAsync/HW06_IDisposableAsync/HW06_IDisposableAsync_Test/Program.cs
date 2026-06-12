using System.Net.Sockets;
using System.Text;

internal class Program
{
    private const string _ip = "127.0.0.1";
    private const int _port = 8080;

    private static async Task Main(string[] args)
    {
        var tests = new List<string>()
        {
            "set user:1(ClientThreadId='{0}'Guid='{1}') true",
            "set user:2(ClientThreadId='{0}'Guid='{1}') false"
        };

        var tstTasks = new List<Task>();

        for (int i = 0; i < 1000; i++)
        {
            foreach (var test in tests)
            {
                tstTasks.Add(Task.Run(() => SendTest(string.Format(test, Thread.CurrentThread.ManagedThreadId, Guid.NewGuid()))));
            }
        }

        Task.WaitAll(tstTasks);

        await Task.Run(() => SendTest($@""));

        Console.WriteLine("Test End");

        Console.ReadKey();
    }

    private static void SendTest(string message)

    {
        try
        {
            Console.WriteLine($@"Send message - '{message}'");

            using (var client = new TcpClient())
            {
                client.Connect(_ip, _port);

                using (var stream = client.GetStream())
                {
                    var data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendTestError: {ex.Message}");
        }
    }
}