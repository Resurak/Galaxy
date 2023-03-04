using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public class GalaxyServer : GalaxyClient
    {
        public GalaxyServer() : base()
        {
            ServerStatus = ServerStatus.Stopped;

            TcpServer = new TcpServer();
            Thread = new Thread(DoServer_Thread);
        }

        Thread Thread;
        TcpServer TcpServer;

        public event Action? Started;
        public event Action? Stopped;

        public ServerStatus ServerStatus { get; private set; }

        public void StartServer()
        {
            if (Thread.ThreadState == ThreadState.Running || TcpServer.Active)
            {
                Log.Warning("Server already executing".AddCaller());
                return;
            }

            TcpServer.Start();
            OnStarted();
        }

        void DoServer_Thread()
        {
            while (ServerStatus == ServerStatus.Running)
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    AcceptConnection();
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        void AcceptConnection()
        {
            try
            {
                TcpClient = TcpServer.AcceptTcpClient();
                NetStream = TcpClient.GetStream();

                OnConnected();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while accepting connection");
            }
        }

        void OnStarted()
        {
            ServerStatus = ServerStatus.Running;
            Thread.Start();

            Started?.Invoke();
        }

        void OnStopped()
        {
            ServerStatus = ServerStatus.Stopped;
            Stopped?.Invoke();
        }

        public void Stop()
        {
            Disconnect();

            TcpServer.Stop();
            OnStopped();
        }
    }

    public enum ServerStatus
    {
        Running,
        Stopped,

        Error
    }
}
