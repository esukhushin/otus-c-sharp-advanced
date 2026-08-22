using HW21_Roslyn_TcpServer.Helper;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace HW21_Roslyn_TcpServer
{
    public class TcpServer : IDisposable
    {
        private bool _disposed = false;
        private readonly Socket _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;
        private readonly SimpleStore _simpleStore;

        private byte[] OK =  Encoding.UTF8.GetBytes("OK\r\n");
        private byte[] ERROR = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");
        private byte[] NIL = Encoding.UTF8.GetBytes("(nil)\r\n");

        public TcpServer(SimpleStore simpleStore)
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
            IMemoryOwner<byte>? memoryOwner = null;
            try
            {
                memoryOwner = _pool.Rent(1024);

                while (!token.IsCancellationRequested)
                {
                    var count = await socket.ReceiveAsync(memoryOwner.Memory, SocketFlags.None);
                    if (count == 0)
                        break;

                    var result = CommandParser.Parse(memoryOwner.Memory.Slice(0, count).Span);
                    if (result.Command.Length == 0)
                        break;

                    var command = Encoding.UTF8.GetString(result.Command).ToLower();
                    switch (command)
                    {
                        case "get":
                            var value = _simpleStore.Get(Encoding.UTF8.GetString(result.Key));
                            if (value != null)
                            {
                                memoryOwner.Memory.Span.Clear();
                                using var stream = GetMemoryStream(memoryOwner);
                                value.SerializeToBinary(stream);
                                await SendAnswerToClient(socket, memoryOwner.Memory, token, (int)stream.Position);
                            }
                            else
                                await SendAnswerToClient(socket, NIL, token);
                            break;
                        case "set":
                            _simpleStore.Set(Encoding.UTF8.GetString(result.Key), UserProfile.DeserializeData(result.Value));
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
                if (memoryOwner != null)
                    memoryOwner.Dispose();

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private async Task SendAnswerToClient(Socket socket, Memory<byte> data, CancellationToken token, int? position = null)
        {
            await socket.SendAsync(position == null ? data : data.Slice(0, position.Value), SocketFlags.None, token);
        }

        public MemoryStream GetMemoryStream(IMemoryOwner<byte> memoryOwner)
        {
            var memory = memoryOwner.Memory;

            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment))
                return new MemoryStream(segment.Array, segment.Offset, segment.Count);
            
            return new MemoryStream(memory.ToArray());
        }
    }
}
