using InMemoryKeyValueCache.TcpServer.NBomber.Tests;
internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await new InMemoryCacheNbomberTest().Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}