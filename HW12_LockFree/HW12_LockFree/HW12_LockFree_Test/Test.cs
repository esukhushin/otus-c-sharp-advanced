using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HW12_LockFree_Test
{
    public class Test
    {
        private const string _ip = "127.0.0.1";
        private const int _port = 8080;
        private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        public async Task SendTest(string message)
        {
            byte[]? bytes = null;

            try
            {
                Console.WriteLine($@"Send message - '{message}'");

                using (var client = new TcpClient())
                {
                    client.Connect(_ip, _port);

                    bytes = _pool.Rent(1024);

                    using (var stream = client.GetStream())
                    {
                        var data = Encoding.UTF8.GetBytes(message);

                        await stream.WriteAsync(data, 0, data.Length);

                        var count = await stream.ReadAsync(bytes);

                        Console.WriteLine($@"Result: {Encoding.UTF8.GetString(bytes.AsSpan(0, count))}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendTestError: {ex.Message}");
            }
            finally
            {
                if (bytes != null)
                    _pool.Return(bytes, true);
            }
        }
    }
}
