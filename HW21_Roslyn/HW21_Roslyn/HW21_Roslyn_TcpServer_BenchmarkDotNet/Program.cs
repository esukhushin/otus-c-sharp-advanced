using HW21_Roslyn_TcpServer_BenchmarkDotNet.Helper;
internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            new TestCase().Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}
