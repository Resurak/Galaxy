using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public static class Utils
    {
        public static byte[]? Serialize(object obj)
        {
            try
            {
                return MessagePackSerializer.Typeless.Serialize(obj);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while serializing data");
                return null;
            }
        }

        public static dynamic? Deserialize(byte[] json)
        {
            try
            {
                return MessagePackSerializer.Typeless.Deserialize(json);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while deserializing data");
                return null;
            }
        }
    }
}
