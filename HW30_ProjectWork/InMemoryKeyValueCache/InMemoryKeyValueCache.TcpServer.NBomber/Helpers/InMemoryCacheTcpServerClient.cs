using InMemoryKeyValueCache.Common.Helpers;
using InMemoryKeyValueCache.Common.Models;
using System.Buffers;
using System.Net.Sockets;
using System.Text;

namespace InMemoryKeyValueCache.TcpServer.NBomber.Helpers
{
    internal class InMemoryCacheTcpServerClient
    {
        private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;
        private readonly byte[] _whiteSpace = Encoding.UTF8.GetBytes(" ");
        private readonly byte[] _set = Encoding.UTF8.GetBytes("set");
        private readonly byte[] _get = Encoding.UTF8.GetBytes("get");

        public async Task<byte[]> SetAsync(string key, UserProfile value)
        {
            byte[]? bytes = null;

            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(ConfigData.IP, ConfigData.Port);
                    bytes = _pool.Rent(Constraints.RentTotalBytes);

                    using (var stream = client.GetStream())
                    {
                        using var streamData = new MemoryStream();
                        value.SerializeToBinary(streamData);

                        var data = new List<byte[]>()
                        {
                            _set,
                            _whiteSpace,
                            Encoding.UTF8.GetBytes(key),
                            _whiteSpace,
                            streamData.ToArray()
                        }
                        .SelectMany(s => s)
                        .ToArray();

                        await stream.WriteAsync(data, 0, data.Length);

                        if (IsConnected(client.Client))
                        {
                            try
                            {
                                var count = await stream.ReadAsync(bytes);
                                return bytes.AsSpan(0, count).ToArray();
                            }
                            catch (Exception)
                            {
                                return Array.Empty<byte>();
                            }
                        }
                        else
                            return Array.Empty<byte>();
                    }
                }
            }
            finally
            {
                if (bytes != null)
                    _pool.Return(bytes, true);
            }
        }
        public async Task<UserProfile?> GetAsync(string key)
        {
            byte[]? bytes = null;

            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(ConfigData.IP, ConfigData.Port);
                    bytes = _pool.Rent(Constraints.RentTotalBytes);

                    using (var stream = client.GetStream())
                    {
                        var data = new List<byte[]>()
                        {
                            _get,
                            _whiteSpace,
                            Encoding.UTF8.GetBytes(key)
                        }
                        .SelectMany(s => s)
                        .ToArray();

                        await stream.WriteAsync(data, 0, data.Length);

                        if (IsConnected(client.Client))
                        {
                            try
                            {
                                var count = await stream.ReadAsync(bytes);
                                return UserProfile.DeserializeData(bytes);
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        }
                        else
                            return null;

                        
                    }
                }
            }
            finally
            {
                if (bytes != null)
                    _pool.Return(bytes, true);
            }
        }
        private bool IsConnected(Socket socket)
        {
            if (socket == null || !socket.Connected)
                return false;

            if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
                return false;

            return true;
        }
    }
}
