using HW12_LockFree_Test;
using System.Buffers;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var test = new Test();

        await test.SendTest("set user:1 true");
        await test.SendTest("get user:1");
        await test.SendTest("get user:2");
        await test.SendTest("delete user:1");
        await test.SendTest("get user:1");
        await test.SendTest("blalbla user:1");

        Console.ReadKey();
    }

    
}