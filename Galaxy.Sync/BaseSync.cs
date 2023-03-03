using Galaxy.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class BaseSync
    {
        public BaseSync()
        {
            Worker = new Thread(ServiceLoop);

            LocalStorages = new List<SyncStorage>();
            RemoteStorages = new List<SyncStorage>();
        }

        Thread Worker;

        NetStream_old? Stream;
        TcpClient? Client;

        public List<SyncStorage> LocalStorages { get; set; }
        public List<SyncStorage> RemoteStorages { get; set; }

        protected void StartWorker()
        {
            Worker.Start();
        }

        async void ServiceLoop()
        {
            while (Stream?.IsConnected ?? false)
            {
                var packet = await Stream.ReceivePacketAsync();
                if (packet.Status != ConnectionStatus.Alive)
                {
                    Log.Warning("Received packet with {name}: {status}".AddCaller(), nameof(ConnectionStatus), packet.Status);
                    continue;
                }

                await HandlePacket(packet);
            }

            //await Disconnect();
            Log.Verbose("Loop ended successfully, closing thread".AddCaller());
        }

        async Task HandlePacket(NetPacket packet)
        {
            if (!packet.HasData)
            {
                Log.Warning("Can't handle packet because it doesn't have any data".AddCaller());
                return;
            }

            var obj = packet.Data.Deserialize();
            if (obj is List<SyncStorage> storages)
            {
                this.RemoteStorages = storages;
                Log.Information("Received available remote storages".AddCaller());
            }
            else if (obj is SyncStorage storage)
            {
                this.RemoteStorages.Add(storage);
                Log.Information("Received available remote storage".AddCaller());
            }
            else
            {
                Log.Warning("Unknown object received. Json:\n{@obj}".AddCaller(), obj);
            }
        }

        //public async Task Disconnect()
        //{
        //    if (Stream != null)
        //    {
        //        await Stream.Disconnect();
        //        Stream = null;

        //        Client?.Dispose();
        //        Client = null;
        //    }
        //}
    }

    public enum SyncFlag
    {
        Updated,
        Outdated,

        Exclusion,
    }
}
