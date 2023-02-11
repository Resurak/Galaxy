using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public class NetPacket
    {
        public NetPacket()
        {

        }

        public NetPacket(ConnectionStatus status)
        {
            Status = status;
        }

        public NetPacket(byte[]? data, ConnectionStatus status)
        {
            Data = data;
            Status = status;
        }

        public byte[]? Data { get; private set; }
        public ConnectionStatus Status { get; private set; }

        public dynamic? GetObject()
        {
            if (Data != null)
            {
                return Utils.Deserialize(Data);
            }
            else
            {
                return null;
            }
        }
    }

    public enum ConnectionStatus
    {
        Alive,
        Error,
        KeepAlive,

        Cancelled,
        Disconnected,
    }
}
