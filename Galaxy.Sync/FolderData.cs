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
            this.Name = "!Root";
            this.FullName = "***";

            this.Files = new List<FileData>();
            this.SubFolders = new List<FolderData>();
        }

        public FolderData(string path) : this() 
        {
            this.FullName = path;
        }

        public FolderData(string name, string fullName) : this(fullName)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public string FullName { get; set; }

        public List<FileData> Files { get; set; }
        public List<FolderData> SubFolders { get; set; }

        public async Task Update()
        {
            try
            {
                var info = new DirectoryInfo(FullName);
                GetFiles(info);

                var tasks = new List<Task>();
                var folders = new List<FolderData>();

                foreach (var folder in info.EnumerateDirectories())
                {
                    var data = new FolderData(folder.Name, folder.FullName);
                    var task = data.Update();

                    folders.Add(data);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
                this.SubFolders.AddRange(folders);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Exception thrown while updating {name} of {path}".AddCaller(), nameof(FolderData), this.FullName);
            }
        }

        void GetFiles(DirectoryInfo info)
        {
            this.Files.Clear();
            foreach (var file in info.EnumerateFiles())
            {
                try
                {
                    var data = new FileData(file);
                    this.Files.Add(data);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Exception thrown while creating {name} of {path}".AddCaller(), nameof(FileData), file.FullName);
                }
            }
        }
    }
}
