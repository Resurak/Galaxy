using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons.Exceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException() : base("Error while client was connected") { }

        public ConnectionException(SocketException ex) : base("Error while client was connected", ex) { }
    }
}
