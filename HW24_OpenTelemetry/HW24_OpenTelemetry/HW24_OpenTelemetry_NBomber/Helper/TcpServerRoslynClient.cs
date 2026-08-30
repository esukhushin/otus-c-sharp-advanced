using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using HW24_OpenTelemetry.Helper;
using HW24_OpenTelemetry_NBomber.Interface;

namespace HW24_OpenTelemetry_NBomber.Helper
{
    public class TcpServerRoslynClient : ITcpServerClient
    {
        private const string _ip = "127.0.0.1";
        private const int _port = 8081;

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
                    await client.ConnectAsync(_ip, _port);
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
                    await client.ConnectAsync(_ip, _port);
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

                        var count = await stream.ReadAsync(bytes);
                        return UserProfile.DeserializeData(bytes);
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
