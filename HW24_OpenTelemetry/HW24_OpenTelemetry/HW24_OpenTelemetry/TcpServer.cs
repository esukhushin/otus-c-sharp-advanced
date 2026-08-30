using HW24_OpenTelemetry.Helper;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace HW24_OpenTelemetry
{
    public class TcpServer : IDisposable
    {
        private int _maxClientSemaphore = 10;
        private bool _disposed = false;
        private readonly Socket _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;
        private readonly SimpleStore _simpleStore;
        private readonly SemaphoreSlim _semaphoreSlim;

        private byte[] OK = Encoding.UTF8.GetBytes("OK\r\n");
        private byte[] ERROR = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");
        private byte[] NIL = Encoding.UTF8.GetBytes("(nil)\r\n");


        public TcpServer(SimpleStore simpleStore)
        {
            _simpleStore = simpleStore;
            _semaphoreSlim = new SemaphoreSlim(_maxClientSemaphore, _maxClientSemaphore);
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
        public async Task StartAsync(string ip, int port, ActivityContext? activityContext, CancellationToken token)
        {
            InitTcpSocket(ip, port);

            Activity? activity = null;
            try
            {
                activity = OTelemetry.ActivitySource.StartActivity(nameof(StartAsync), ActivityKind.Internal, activityContext ?? default);
                while (!token.IsCancellationRequested)
                {
                    var clientSocket = await _tcpSocket.AcceptAsync(token);

                    if (await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(1), token))
                        await ProcessClientAsync(clientSocket, activity?.Context, token);
                }
            }
            catch (OperationCanceledException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
            finally
            {
                activity?.Dispose();
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
        private async Task ProcessClientAsync(Socket socket, ActivityContext? activityContext, CancellationToken token)
        {
            IMemoryOwner<byte>? memoryOwner = null;
            Activity? activity = null;
            Stopwatch? stopwatch = null;

            try
            {
                activity = OTelemetry.ActivitySource.StartActivity(nameof(ProcessClientAsync), ActivityKind.Internal, activityContext ?? default);
                stopwatch = Stopwatch.StartNew();

                memoryOwner = _pool.Rent(Constraints.RentTotalBytes);
                while (!token.IsCancellationRequested)
                {
                    var dataResult = await GetClientDataAsync(socket, memoryOwner.Memory, token);
                    if (!dataResult.status)
                    {
                        activity?.SetTag($@"Command.GetClientDataAsync.Request(max - {Constraints.TotalBytes})", dataResult.count);
                        break;
                    }

                    if (dataResult.count == 0)
                        break;
                    
                    var result = CommandParser.Parse(memoryOwner.Memory.Slice(0, dataResult.count).Span);
                    if (result.Command.Length == 0)
                    {
                        activity?.SetTag("Command.CommandParser.Command.Length", result.Command.Length);
                        break;
                    }
                        
                    var command = Encoding.UTF8.GetString(result.Command).ToLower();
                    activity?.SetTag("Command.Name", command);
                    switch (command)
                    {
                        case "get":
                            var key = Encoding.UTF8.GetString(result.Key);
                            activity?.SetTag("Command.Key", key);

                            var value = _simpleStore.Get(key);
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
                            key = Encoding.UTF8.GetString(result.Key);
                            activity?.SetTag("Command.Key", key);

                            _simpleStore.Set(Encoding.UTF8.GetString(result.Key), UserProfile.DeserializeData(result.Value));
                            await SendAnswerToClient(socket, OK, token);
                            break;
                        case "delete":
                            key = Encoding.UTF8.GetString(result.Key);
                            activity?.SetTag("Command.Key", key);

                            _simpleStore.Delete(Encoding.UTF8.GetString(result.Key));
                            await SendAnswerToClient(socket, OK, token);
                            break;
                        default:
                            await SendAnswerToClient(socket, ERROR, token);
                            break;
                    }
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
            finally
            {
                stopwatch?.Stop();

                OTelemetry.Counter.Add(1);
                OTelemetry.Duration.Record(stopwatch?.Elapsed.TotalMilliseconds ?? -1);

                memoryOwner?.Dispose();
                activity?.Dispose();

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                _semaphoreSlim.Release();
            }
        }
        private async Task SendAnswerToClient(Socket socket, Memory<byte> data, CancellationToken token, int? position = null)
        {
            await socket.SendAsync(position == null ? data : data.Slice(0, position.Value), SocketFlags.None, token);
        }
        private MemoryStream GetMemoryStream(IMemoryOwner<byte> memoryOwner)
        {
            var memory = memoryOwner.Memory;

            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment))
                return new MemoryStream(segment.Array, segment.Offset, segment.Count);

            return new MemoryStream(memory.ToArray());
        }
        private async Task<(bool status, int count)> GetClientDataAsync(Socket socket, Memory<byte> data, CancellationToken token)
        {
            var count = await socket.ReceiveAsync(data, SocketFlags.None);

            if (count >= Constraints.TotalBytes)
                return (false, count);

            return (true, count);
        }
    }
}
