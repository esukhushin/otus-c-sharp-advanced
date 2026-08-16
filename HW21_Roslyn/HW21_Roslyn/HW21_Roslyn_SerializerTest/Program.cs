using HW21_Roslyn_SerializerTest;
using System;
using System.IO;
using System.Reflection;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var whiteSpace = Encoding.UTF8.GetBytes(" ");
        var set = Encoding.UTF8.GetBytes("set");
        var get = Encoding.UTF8.GetBytes("get");

        var _rnd = new Random(10000);

        for (int i = 0; i < 10000; ++i)
        {
            var key = $@"user:{Guid.NewGuid()}";
            var value = new UserProfile()
            {
                Id = _rnd.Next(),
                Username = $@"User:{Guid.NewGuid()}",
                CreatedAt = DateTime.Now
            };

            var data = new List<byte[]>()
            {
                set,
                whiteSpace,
                Encoding.UTF8.GetBytes(key),
                whiteSpace,
                UserProfile.ConvertToByteArray(value)
            }
            .SelectMany(s => s)
            .ToArray();

            var parsed = HW21_Roslyn_TcpServer.Helper.CommandParser.Parse(data);
            var result = UserProfile.DeserializeData(parsed.Value);

            Console.WriteLine($@"{result?.Id} - {result?.Username} - {result?.CreatedAt}");

        }
    }
}