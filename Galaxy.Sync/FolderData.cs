using Galaxy.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class FolderData
    {
        public FolderData() 
        {
            this.Path = "***";
            this.Flags = new List<SyncFlag>();

            this.Files = new List<FileData>();
            this.SubFolders = new List<FolderData>();
        }

        public FolderData(string path) : this() 
        {
            this.Path = path;
        }

        public string Path { get; set; }
        public List<SyncFlag> Flags { get; set; }

        public List<FileData> Files { get; set; }
        public List<FolderData> SubFolders { get; set; }

        public void UpdateFiles(DirectoryInfo info)
        {
            this.Files.Clear();
            foreach (var file in info.EnumerateFiles())
            {
                try
                {
                    var data = new FileData(Path, file);
                    this.Files.Add(data);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Exception thrown while creating {name} of {path}".AddCaller(), nameof(FileData), file.FullName);
                }
            }
        }

        public async Task UpdateFolder()
        {
            try
            {
                var info = new DirectoryInfo(Path);
                UpdateFiles(info);

                var tasks = new List<Task>();
                var folders = new List<FolderData>();

                foreach (var folder in info.EnumerateDirectories())
                {
                    var data = new FolderData(folder.FullName);
                    var task = data.UpdateFolder();

                    folders.Add(data);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
                this.SubFolders.AddRange(folders);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while updating {name} of {path}".AddCaller(), nameof(FolderData), this.Path);
            }
        }
    }
}
