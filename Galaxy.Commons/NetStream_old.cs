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
    public class NetStream_old : NetworkStream
    {
        public NetStream_old(TcpClient client) : base(client.Client) 
        {
            IsConnected = true;
            InternalSizeBuffer = new byte[sizeof(int)];
        }

        byte[] InternalSizeBuffer;

        public bool IsConnected { get; private set; }

        public async Task<NetPacket> ReceivePacketAsync(int lenght = -1, CancellationToken token = default)
        {
            if (!IsConnected)
            {
                return new NetPacket(ConnectionStatus.Error);
            }

            try
            {
                if (lenght >= 0)
                {
                    var buffer = new byte[lenght];
                    await ReadExactlyAsync(buffer, 0, lenght);

                    return new NetPacket(buffer, ConnectionStatus.Alive);
                }
                else
                {
                    await ReadExactlyAsync(InternalSizeBuffer, token);
                    var size = BitConverter.ToInt32(InternalSizeBuffer, 0);

                    if (size == 0 || size < -1)
                    {
                        // received incorrect buffer size

                        return new NetPacket(ConnectionStatus.Error);
                    }

                    var buffer = new byte[size];
                    await ReadExactlyAsync(buffer, token);

                    return new NetPacket(buffer, ConnectionStatus.Alive);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Log.Debug("Cancelled".AddCaller());
                    return new NetPacket(ConnectionStatus.Cancelled);
                }

                if (ex is IndexOutOfRangeException)
                {
                    Log.Debug("Connection returned 0 byte".AddCaller());
                }
                else if (ex is EndOfStreamException or SocketException or IOException)
                {
                    Log.Debug("Connection closed".AddCaller());
                }
                else
                {
                    Log.Warning(ex, "Unhandled exception thrown".AddCaller());
                }

                Dispose();
                return new NetPacket(ConnectionStatus.Disconnected);
            }
            finally
            {

            }
        }

        public async Task<ConnectionStatus> SendAsync(byte[] data, CancellationToken token = default)
        {
            if (!IsConnected)
            {
                return ConnectionStatus.Error;
            }

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
                    Log.Debug("Cancelled".AddCaller());
                    return ConnectionStatus.Cancelled;
                }

                if (ex is IndexOutOfRangeException)
                {
                    Log.Debug("Connection returned 0 byte".AddCaller());
                }
                else if (ex is EndOfStreamException or SocketException or IOException)
                {
                    Log.Debug("Connection closed".AddCaller());
                }
                else
                {
                    Log.Warning(ex, "Unhandled exception thrown".AddCaller());
                }

                Dispose();
                return ConnectionStatus.Disconnected;
            }
            finally
            {

            }
        }

        public new void Dispose()
        {
            base.Dispose();
            IsConnected = false;

            Log.Information("Connection closed".AddCaller());
        }
    }
}
