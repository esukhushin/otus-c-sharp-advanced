using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleStore09 = HW09_Channels;
using HW03_SpanMemoryStackalloc;

namespace HW12_LockFree
{
    public class TcpServer : IDisposable
    {
        private bool _disposed = false;
        private readonly Socket _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;
        private readonly SimpleStore09.SimpleStore _simpleStore;

        private const string OK = "OK\r\n";
        private const string ERROR = "-ERR Unknown command\r\n";
        private const string NIL = "(nil)\r\n";

        public TcpServer(SimpleStore09.SimpleStore simpleStore)
        {
            _simpleStore = simpleStore;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _tcpSocket.Dispose();
            }

            _disposed = true;
        }

        public async Task StartAsync(string ip, int port, CancellationToken token)
        {
            InitTcpSocket(ip, port);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var clientSocket = await _tcpSocket.AcceptAsync(token);

                    await ProcessClientAsync(clientSocket, token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($@"StartAsyncCanceled");
                throw;
            }
        }

        private void InitTcpSocket(string ip, int port)
        {
            _tcpSocket.Bind(new IPEndPoint(GetIP(ip), port));
            _tcpSocket.Listen();
        }

        private IPAddress GetIP(string ip)
        {
            if (!IPAddress.TryParse(ip, out var address))
                throw new InvalidDataException("ip address is incorrect");

            return address;
        }

        private async Task ProcessClientAsync(Socket socket, CancellationToken token)
        {
            byte[]? arrayByte = null;
            try
            {
                arrayByte = _pool.Rent(1024);

                while (!token.IsCancellationRequested)
                {
                    var count = await socket.ReceiveAsync(arrayByte, SocketFlags.None);
                    if (count == 0)
                        break;

                    var result = CommandParser.Parse(arrayByte.AsSpan(0, count));
                    if (result.Command.Length == 0)
                        break;

                    var command = Encoding.UTF8.GetString(result.Command).ToLower();
                    switch (command)
                    {
                        case "get":
                            var value = _simpleStore.Get(Encoding.UTF8.GetString(result.Key));
                            if (value != null)
                                await SendAnswerToClient(socket, $@"{Encoding.UTF8.GetString(value)}{Environment.NewLine}", token);
                            else
                                await SendAnswerToClient(socket, NIL, token);
                            break;
                        case "set":
                            _simpleStore.Set(Encoding.UTF8.GetString(result.Key), result.Value.ToArray());
                            await SendAnswerToClient(socket, OK, token);
                            break;
                        case "delete":
                            _simpleStore.Delete(Encoding.UTF8.GetString(result.Key));
                            await SendAnswerToClient(socket, OK, token);
                            break;
                        default:
                            await SendAnswerToClient(socket, ERROR, token);
                            break;
                    }
                }
            }
            finally
            {
                if (arrayByte != null)
                    _pool.Return(arrayByte, true);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private async Task SendAnswerToClient(Socket socket, string message, CancellationToken token)
        {
            await socket.SendAsync(Encoding.UTF8.GetBytes(message), SocketFlags.None, token);
        }
    }
}
