using HW21_Roslyn_TcpServer_NBomber.Helper;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            new TestCaseTcpServer().Execute();
            //await new TestCaseExecuteSetGet().Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}