using Galaxy.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class SyncShare
    {
        public SyncShare()
        {
            this.Token = Guid.NewGuid();
            this.RequireAuth = false;

            this.UpdateThread = new Thread(UpdateWorker);
        }

        public SyncShare(string path, string name) : this()
        {
            this.Path = path;
            this.Name = name;
        }

        public event Action? Updated;

        public string Path { get; set; }
        public string Name { get; set; }

        public Guid Token { get; set; }
        public bool RequireAuth { get; set; }

        public FolderData FolderData { get; set; }

        Thread UpdateThread;

        public void Update()
        {
            if (UpdateThread.ThreadState == ThreadState.Running)
            {
                Log.Debug("Thread busy updating".AddCaller());
                return;
            }

            Log.Verbose("Updating {name} of {path}".AddCaller(), nameof(SyncShare), Path);
            UpdateThread.Start();
        }

        async void UpdateWorker()
        {
            FolderData = new FolderData(Path);
            await FolderData.Update();

            Updated?.Invoke();
        }
    }
}
