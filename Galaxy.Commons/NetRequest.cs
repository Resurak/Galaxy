using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public class NetRequest
    {
        public NetRequest() 
        {
            this.ID = Guid.NewGuid();
        }

        public NetRequest(RequestCode code) : this()
        {
            this.Code = code;
        }

        public NetRequest(RequestCode code, string header) : this(code)
        {
            this.Header = header;
        }

        public NetRequest(RequestCode code, object payload) : this(code)
        {
            this.Payload = payload;
        }

        public Guid ID { get; set; }
        public RequestCode Code { get; set; }

        public string Header { get; set; }
        public object Payload { get; set; }
    }

    public enum RequestCode
    {
        Custom,

        Auth,
        SSLEncrypt,

        Handshake,
        Disconnect,
    }
}
