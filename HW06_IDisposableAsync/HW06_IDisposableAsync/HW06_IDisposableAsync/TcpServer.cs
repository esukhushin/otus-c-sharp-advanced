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
        ~TcpServer()
        {
            Dispose(false);
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

        public async Task StartAsync(string ip, int port)
        {
            try
            {
                InitTcpSocket(ip, port);

                var ct = new CancellationTokenSource();

                while (true)
                {
                    var clientSocket = await _tcpSocket.AcceptAsync();

                    var clientTask = await Task.Factory.StartNew(() => ProcessClientAsync(clientSocket, ct),
                        TaskCreationOptions.AttachedToParent);

                    if (ct.IsCancellationRequested)
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                this.Dispose(true);
            }

        }

        private void InitTcpSocket(string ip, int port)
        {
            _tcpSocket.Bind(new IPEndPoint(GetIP(ip), port));
            _tcpSocket.Listen();
        }

        private IPAddress GetIP(string ip)
        {
            var regex = new Regex(@"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
            if (!regex.IsMatch(ip))
                throw new InvalidDataException("ip address is incorrect");

            return IPAddress.Parse(ip);
        }

        private async Task ProcessClientAsync(Socket socket, CancellationTokenSource cancellationToken)
        {
            byte[]? arrayByte = null;
            try
            {
                arrayByte = _pool.Rent(1024);

                var count = await socket.ReceiveAsync(arrayByte, SocketFlags.None);
                if (count == 0)
                {
                    cancellationToken.Cancel();
                    return;
                }

                var result = CommandParser.Parse(arrayByte);

                Console.WriteLine($@"ServerThreadId - '{Thread.CurrentThread.ManagedThreadId}' Command - '{Encoding.UTF8.GetString(result.Command)}' Key - '{Encoding.UTF8.GetString(result.Key)}' Value - '{Encoding.UTF8.GetString(result.Value)}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error - {ex.Message}");
            }
            finally
            {
                if (arrayByte != null)
                    _pool.Return(arrayByte, true);

                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
            }
        }
    }
}