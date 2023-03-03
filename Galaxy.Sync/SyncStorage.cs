using Galaxy.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class SyncStorage
    {
        public SyncStorage() 
        {
            this.ID = Guid.NewGuid();
            this.updater = new Thread(async () => await BackgroundUpdate());
        }

        public SyncStorage(string root) : this() 
        {
            this.Path = root;
            this.RootFolder = new FolderData(root);
        }

        Thread updater;

        public Guid ID { get; private set; }
        public string Path { get; private set; }

        public FolderData RootFolder { get; private set; }

        public DateTime LastUpdateTime { get; private set; }

        public void Update()
        {
            if (updater.ThreadState == System.Threading.ThreadState.Running)
            {
                Log.Verbose("{path} || Can't update because it's already updating".AddCaller(), this.Path);
                return;
            }

            if (RootFolder == null)
            {
                Log.Verbose("{path} || Can't update because path is not set".AddCaller(), this.Path);
                return;
            }

            updater.Start();
        }

        private async Task BackgroundUpdate()
        {
            try
            {
                Log.Verbose("{path} || Updating".AddCaller(), this.Path);

                var sw = Stopwatch.StartNew();
                await RootFolder.UpdateFolder();

                sw.Stop();
                Log.Verbose("{path} || Updated in {ms}ms".AddCaller(), this.Path, sw.ElapsedMilliseconds);

                LastUpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "{path} || Exception thrown while updating".AddCaller(), this.Path);
            }
        }
    }
}
