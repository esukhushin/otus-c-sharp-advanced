using HW24_OpenTelemetry.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HW24_OpenTelemetry_NBomber.Helper
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
                Username = GetUserName(),
                CreatedAt = DateTime.Now
            };

            var client = new TcpServerRoslynClient();

            await client.SetAsync(key1, value1);
            await client.SetAsync(key2, value2);

            var responseGet = await client.GetAsync(key1);
            Console.WriteLine($@"{responseGet?.Id} - {responseGet?.Username} - {responseGet?.CreatedAt}");
        }

        private string GetUserName()
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append("User:");
            for (int i = 0; i < 1000; i++)
                strBuilder.Append(Guid.NewGuid());
            return strBuilder.ToString();
        }
    }
}
