using HW18_SIMD_Test;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await new TestCase().StartTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}