using Galaxy.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class FileData
    {
        public FileData()
        {

        }

        public FileData(FileInfo info)
        {
            this.Size = info.Length;
            this.Name = info.Name;

            this.CreationTime = info.CreationTime;
            this.LastWriteTime = info.LastWriteTime;
        }

        public long Size { get; set; }
        public string Name { get; set; }

        public DateTime CreationTime { get; private set; }
        public DateTime LastWriteTime { get; private set; }
    }
}
