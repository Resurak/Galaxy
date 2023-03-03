using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public delegate void UpdateEventHandler(object? state);

    public static class Utils
    {
        public static byte[]? Serialize(this object obj)
        {
            try
            {
                return MessagePackSerializer.Typeless.Serialize(obj);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while serializing data".AddCaller());
                return null;
            }
        }

        public static dynamic? Deserialize(this byte[] data)
        {
            try
            {
                return MessagePackSerializer.Typeless.Deserialize(data);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while deserializing data".AddCaller());
                return null;
            }
        }

        public static string AddCaller(this string log, [CallerMemberName] string caller = "", [CallerFilePath] string file = "") =>
            $"{Path.GetFileNameWithoutExtension(file)}.{caller} || {log}";
    }
}
