using HW03_SpanMemoryStackalloc;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace HW06_IDisposableAsync
{
    public class TcpServer : IDisposable
    {
        private bool _disposed = false;
        private readonly Socket _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        public TcpServer()
        {

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
            if(!IPAddress.TryParse(ip, out var address))
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

                    Console.WriteLine($@"ServerThreadId - '{Thread.CurrentThread.ManagedThreadId}' Command - '{Encoding.UTF8.GetString(result.Command)}' Key - '{Encoding.UTF8.GetString(result.Key)}' Value - '{Encoding.UTF8.GetString(result.Value)}'");
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
    }
}