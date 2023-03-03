using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public delegate void DataEventHandler(byte[] data);

    public class GalaxyClient
    {
        public GalaxyClient() 
        {
            Worker = new Thread(ConnectionLoop);
        }

        Thread Worker;

        TcpClient? TcpClient;
        NetworkStream? NetStream;

        public event Action? Connected;
        public event Action? Disconnected;
        public event DataEventHandler? DataReceived;

        public NetStatus Status { get; set; }

        public async Task ConnectAsync(string address = "127.0.0.1", int port = 6969)
        {
            try
            {
                var ip = IPAddress.Parse(address);
                var endPoint = new IPEndPoint(ip, port);

                TcpClient = new TcpClient();
                await TcpClient.ConnectAsync(endPoint);

                NetStream = new NetworkStream(TcpClient.Client);
                Status = NetStatus.Connected;

                Worker.Start();
                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while connecting to {address}:{port}".AddCaller(), address, port);
            }
            finally
            {
                if (Status != NetStatus.Connected)
                {
                    Close();
                }
            }
        }

        void ConnectionLoop()
        {
            while (Status == NetStatus.Connected)
            {
                var data = ReceiveData();
                if (data.Length > 0)
                {
                    DataReceived?.Invoke(data);
                }
            }

            Close();
            Disconnected?.Invoke();
        }

        public byte[] ReceiveData()
        {
            try
            {
                var buffer = new byte[1024 * 128];
                var read = NetStream.Read(buffer);

                return buffer.Take(read).ToArray();
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Log.Verbose("Read operation cancelled".AddCaller());
                }
                else if (ex is ObjectDisposedException or SocketException or IOException or InvalidOperationException)
                {
                    Log.Warning("Tried to receive data but stream is closed".AddCaller());
                }
                else
                {
                    Log.Warning(ex, "Exception thrown while receiving data".AddCaller());
                }

                return new byte[0];
            }
        }

        public async Task<byte[]> ReceiveDataAsync()
        {
            try
            {
                var buffer = new byte[1024 * 128];
                var read = await NetStream.ReadAsync(buffer);

                return buffer.Take(read).ToArray();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return new byte[0];
            }
        }

        public void SendData(byte[] data)
        {
            try
            {

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public async Task SendDataAsync(byte[] data)
        {
            try
            {

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        void HandleException(Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                Log.Verbose("Read operation cancelled".AddCaller());
            }
            else if (ex is ObjectDisposedException or SocketException or IOException or InvalidOperationException)
            {
                Log.Warning("Tried to receive data async but stream is closed".AddCaller());
                Status = NetStatus.Error;
            }
            else
            {
                Log.Warning(ex, "Exception thrown while receiving data".AddCaller());
                Status = NetStatus.Error;
            }
        }

        void Close()
        {
            NetStream?.Dispose();
            TcpClient?.Dispose();

            TcpClient = null;
            NetStream = null;

            Status = NetStatus.Disconnected;
        }
    }

    public enum NetStatus
    {
        Connected,
        Disconnected,

        Error
    }
}
