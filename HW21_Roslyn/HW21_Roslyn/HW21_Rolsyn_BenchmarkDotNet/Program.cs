using HW21_Rolsyn_BenchmarkDotNet.Helper;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            TestCaseSerialize.Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}