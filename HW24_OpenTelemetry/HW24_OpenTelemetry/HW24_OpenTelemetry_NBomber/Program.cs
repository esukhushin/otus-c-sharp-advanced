using HW24_OpenTelemetry_NBomber.Helper;
internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await new TestCaseNBomber().StartTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ReadKey();
    }
}