using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public class NetResponse
    {
        public NetResponse()
        {
            this.ID = Guid.NewGuid();
        }

        public NetResponse(ResponseCode code) : this()
        {
            this.Code = code;
        }

        public NetResponse(ResponseCode code, string header) : this(code)
        {
            this.Header = header;
        }

        public NetResponse(ResponseCode code, object payload) : this(code)
        {
            this.Payload = payload;
        }

        public Guid ID { get; set; }
        public ResponseCode Code { get; set; }

        public string Header { get; set; }
        public object Payload { get; set; }
    }

    public enum ResponseCode
    {
        Accepted,
        Rejected,

        Error_InvalidData,
        Error_InvalidRequest,

        Error_NoData,
        Error_noRequest,

        Error_NotFound,
        Error_NotAuthorized,

        Error_RequireSSL,
        Error_RequireLogin
    }
}
