using InMemoryKeyValueCache.Common.BenchmarkDotNet.Tests;
internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            InMemoryCacheCommonTest.Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}