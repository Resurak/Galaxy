using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class FileBuffer
    {
        public long Length { get; set; }
        public long Position { get; set; }

        public byte[] Data { get; set; }
    }
}
