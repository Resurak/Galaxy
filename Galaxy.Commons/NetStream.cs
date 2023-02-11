using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Galaxy.Commons
{
    public class NetStream : NetworkStream
    {
        public NetStream(TcpClient client, bool keepAlive = true, int interval = 5000) : base(client.Client) 
        {
            KeepAlive = keepAlive;
            TimerInterval = interval > 0 ? interval : 5000;

            InternalTimer = new Timer(TimerCallback, null, TimerInterval, TimerInterval);
            InternalSizeBuffer = new byte[sizeof(int)];
        }

        public bool KeepAlive { get; set; }
        public bool IsConnected { get; private set; }

        bool Disposed;
        bool IsReading;
        bool IsWriting;

        int TimerInterval;
        Timer InternalTimer;

        byte[] InternalSizeBuffer;

        public async Task<NetPacket> ReceivePacketAsync(CancellationToken token = default)
        {
            if (!IsConnected)
            {
                return new NetPacket(ConnectionStatus.Error);
            }

            IsReading = true;

            try
            {
                await ReadExactlyAsync(InternalSizeBuffer, token);
                var size = BitConverter.ToInt32(InternalSizeBuffer, 0);

                if (size == int.MinValue)
                {
                    Dispose();
                    return new NetPacket(ConnectionStatus.Disconnected);
                }

                if (size == -1)
                {
                    return new NetPacket(ConnectionStatus.KeepAlive);
                }

                if (size == 0 || size < -1)
                {
                    return new NetPacket(ConnectionStatus.Error);
                }

                var buffer = new byte[size];
                await ReadExactlyAsync(buffer, token);

                return new NetPacket(buffer, ConnectionStatus.Alive);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Log.Debug("{method} || Cancelled", nameof(ReceivePacketAsync));
                    return new NetPacket(ConnectionStatus.Cancelled);
                }

                if (ex is IndexOutOfRangeException)
                {
                    Log.Debug("{method} || Connection returned 0 byte", nameof(ReceivePacketAsync));
                }
                else if (ex is EndOfStreamException or SocketException or IOException)
                {
                    Log.Debug("{method} || Connection closed", nameof(ReceivePacketAsync));
                }
                else
                {
                    Log.Debug(ex, "{method} || Unhandled exception thrown", nameof(ReceivePacketAsync));
                }

                Dispose();
                return new NetPacket(ConnectionStatus.Disconnected);
            }
            finally
            {
                IsReading = false;
            }
        }

        public async Task<ConnectionStatus> SendAsync(byte[] data, CancellationToken token = default)
        {
            if (!IsConnected)
            {
                return ConnectionStatus.Error;
            }

            IsWriting = true;

            try
            {
                var size = BitConverter.GetBytes(data.Length);

                await WriteAsync(size, token);
                await WriteAsync(data, token);

                return ConnectionStatus.Alive;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Log.Debug("{method} || Cancelled", nameof(SendAsync));
                    return ConnectionStatus.Cancelled;
                }

                if (ex is IndexOutOfRangeException)
                {
                    Log.Debug("{method} || Connection returned 0 byte", nameof(SendAsync));
                }
                else if (ex is EndOfStreamException or SocketException or IOException)
                {
                    Log.Debug("{method} || Connection closed", nameof(SendAsync));
                }
                else
                {
                    Log.Debug(ex, "{method} || Unhandled exception thrown", nameof(SendAsync));
                }

                Dispose();
                return ConnectionStatus.Disconnected;
            }
            finally
            {
                IsWriting = false;
            }
        }

        public async Task Disconnect()
        {
            var ct = new CancellationTokenSource(5000);
            var disconnectionBuffer = BitConverter.GetBytes(int.MinValue);

            _ = await SendAsync(disconnectionBuffer, ct.Token);
        }

        async void TimerCallback(object? state = null)
        {
            if (!KeepAlive || !IsConnected || IsWriting)
            {
                return;
            }

            var sizeBuffer = BitConverter.GetBytes(-1);
            await WriteAsync(sizeBuffer);
        }

        public new void Dispose()
        {
            base.Dispose();
            InternalTimer.Dispose();

            IsConnected = false;
        }
    }
}
