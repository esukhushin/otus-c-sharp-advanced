using HW21_Roslyn_TcpServer.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Roslyn_TcpServer_NBomber.Helper
{
    public class TestCaseExecuteSetGet
    {
        private readonly Random _rnd = new Random(10);
        
        public async Task Execute()
        {
            var key1 = $@"user:{Guid.NewGuid()}";
            var key2 = $@"user:{Guid.NewGuid()}";
            var value1 = new UserProfile()
            {
                Id = _rnd.Next(),
                Username = $@"User:{Guid.NewGuid()}",
                CreatedAt = DateTime.Now
            };
            var value2 = new UserProfile()
            {
                Id = _rnd.Next(),
                Username = $@"User:{Guid.NewGuid()}",
                CreatedAt = DateTime.Now
            };

            var client = new TcpServerRoslynClient();

            await client.SetAsync(key1, value1);
            await client.SetAsync(key2, value2);

            var responseGet = await client.GetAsync(key2);
            Console.WriteLine($@"{responseGet?.Id} - {responseGet?.Username} - {responseGet?.CreatedAt}");
        }
    }
}
