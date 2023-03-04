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
            
        }

        Thread? ConnectThread;
        Thread? ConnectionLoopThread;

        TcpClient? TcpClient;
        NetworkStream? NetStream;

        public event Action? Connected;
        public event Action? Disconnected;
        public event DataEventHandler? DataReceived;

        public int BufferSize { get; set; } = 128 * 1024;

        public StreamFlag StreamFlag { get; set; }
        public ConnectionStatus ConnectionStatus { get; private set; }

        bool _isReading; // not used

        public void Connect(string address = "127.0.0.1", int port = 6969)
        {
            try
            {
                var ip = IPAddress.Parse(address);
                var endPoint = new IPEndPoint(ip, port);

                ConnectThread = new Thread(() => DoConnect_Thread(endPoint));
                ConnectThread.Start();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while connecting to {address}:{port}".AddCaller(), address, port);
            }
        }

        protected void Connect(TcpClient client)
        {
            TcpClient = client;
            NetStream = TcpClient.GetStream();
            OnConnected();

            ConnectionLoopThread = new Thread(DoConnectionLoop_Thread);
            ConnectionLoopThread.Start();
        }

        void DoConnect_Thread(IPEndPoint endPoint)
        {
            Log.Verbose("Thread starting".AddCaller());

            if (ConnectionStatus == ConnectionStatus.Connected)
            {
                Log.Warning("Client already connected".AddCaller());
                return;
            }

            if (ConnectThread != null && ConnectThread.ThreadState == ThreadState.Running)
            {
                Log.Warning("Connection thread is executing, can't connect".AddCaller());
                return;
            }

            Close();

            try
            {
                TcpClient = new TcpClient();
                TcpClient.Connect(endPoint);

                NetStream = TcpClient.GetStream();
                OnConnected();

                ConnectionLoopThread = new Thread(DoConnectionLoop_Thread); 
                ConnectionLoopThread.Start();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while connecting to {address}:{port}".AddCaller(), endPoint.Address.ToString(), endPoint.Port);
            }

            Log.Verbose("Thread closing".AddCaller());
        }

        void DoConnectionLoop_Thread()
        {
            Log.Verbose("Thread started".AddCaller());

            while (ConnectionStatus == ConnectionStatus.Connected)
            {
                if (StreamFlag == StreamFlag.Hold)
                {
                    Thread.Sleep(100);
                    continue;
                }

                var data = ReceiveData();
                if (data.Length > 0)
                {
                    DataReceived?.Invoke(data);
                }
            }

            while (ConnectionStatus == ConnectionStatus.Error)
            {
                Thread.Sleep(1000);
            }

            Close();
            OnDisconnected();

            Log.Verbose("Thread stopped".AddCaller());
        }

        public byte[] ReceiveData()
        {
            _isReading = true;

            try
            {
                var buffer = new byte[BufferSize];
                var read = NetStream.Read(buffer);

                if (read == BufferSize)
                {
                    return buffer;
                }
                else
                {
                    return buffer.Take(read).ToArray();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return new byte[0];
            }
            finally
            {
                _isReading = false;
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                NetStream.Write(data);
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
                await NetStream.WriteAsync(data);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public void Disconnect()
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        void OnConnected()
        {
            ConnectionStatus = ConnectionStatus.Connected;
            Connected?.Invoke();
        }

        void OnDisconnected()
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            Disconnected?.Invoke();
        }

        void Close()
        {
            NetStream?.Dispose();
            TcpClient?.Dispose();

            TcpClient = null;
            NetStream = null;
        }

        void HandleException(Exception ex)
        {
            Log.Verbose("Stream read/write threw an exception".AddCaller());

            if (ex is OperationCanceledException)
            {
                Log.Verbose("Stream operation cancelled".AddCaller());
                return;
            }

            if (ex is ObjectDisposedException or SocketException or IOException or InvalidOperationException)
            {
                Log.Warning("Tried to receive data async but stream is closed".AddCaller());
            }
            else
            {
                Log.Warning(ex, "Exception thrown while receiving data".AddCaller());
            }

            ConnectionStatus = ConnectionStatus.Error;
        }
    }

    public enum StreamFlag
    {
        Hold,
        Receive,
    }

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,

        Error
    }
}
