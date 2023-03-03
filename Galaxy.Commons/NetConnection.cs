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
    public delegate void NetPacketEventHandler(NetPacket packet);

    public class NetConnection
    {
        public NetConnection()
        {
            worker = new Thread(ServiceLoop);

            Client = new TcpClient();
            Server = new TcpServer();
        }

        Thread worker;

        TcpClient Client;
        TcpServer Server;
        NetStream_old? NetStream;

        public event Action? ClientConnected;
        public event Action? ClientDisconnected;

        public event Action? ServerClosed;
        public event Action? ServerStarted;

        public event NetPacketEventHandler? PacketReceived;

        public void StartServer()
        {
            Server.Start();
            ServerStarted?.Invoke();

            worker.Start();
        }

        public void StopServer() 
        {
            CloseConnection().Wait();
            Server.Stop();

            ServerClosed?.Invoke();
        }

        public async Task StartClient()
        {
            await Client.ConnectAsync("127.0.0.1", 6969);
            NetStream = new NetStream_old(Client);

            worker.Start();
        }

        async void ServiceLoop()
        {
            try
            {
                while (NetStream != null && NetStream.IsConnected)
                {
                    var packet = await NetStream.ReceivePacketAsync();
                    if (packet.Status == ConnectionStatus.Alive)
                    {
                        PacketReceived?.Invoke(packet);
                        continue;
                    }

                    if (packet.Status is ConnectionStatus.Cancelled or ConnectionStatus.KeepAlive)
                    {
                        continue;
                    }

                    Log.Warning("Connection status: {status}".AddCaller(), packet.Status);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while receiving packets".AddCaller());
            }
            finally
            {
                await CloseConnection();
            }
        }

        public async Task CloseConnection()
        {
            if (NetStream != null && NetStream.IsConnected)
            {
                var request = new NetRequest(RequestCode.Disconnect);
                _ = await NetStream.SendAsync(request.Serialize());

                NetStream.Dispose();
                NetStream = null;
            }

            Client?.Dispose();
            NetStream?.Dispose();

            Client = new TcpClient();
            NetStream = null;

            ClientDisconnected?.Invoke();
        }
    }
}
