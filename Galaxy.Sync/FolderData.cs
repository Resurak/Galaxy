using Galaxy.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Sync
{
    public class FolderData : INamedElement
    {
        public FileData? this[string[] paths]
        {
            get
            {
                if (paths.Length == 1)
                {
                    return Files[paths[0]];
                }
                else
                {
                    var sub = SubFolders[paths[0]];
                    if (sub != null)
                    {
                        return sub[paths.Skip(1).ToArray()];
                    }
                }

                return null;
            }
        }

        public FolderData() { }

        public FolderData(DirectoryInfo info)
        {
            this.Name = info.Name;

            this.Files = new NamedList<FileData>();
            this.SubFolders = new NamedList<FolderData>();
        }

        public string Root { get; set; }
        public string Name { get; set; }

        public NamedList<FileData> Files { get; private set; }
        public NamedList<FolderData> SubFolders { get; private set; }

        public async Task Create(string path)
        {
            var root = new DirectoryInfo(path);
            var data = await GetFolderData(root);

            this.Root = path;
            this.Name = data.Name;
            this.Files = data.Files;
            this.SubFolders = data.SubFolders;
        }

        async Task<FolderData> GetFolderData(DirectoryInfo rootDirInfo)
        {
            var folderData = new FolderData(rootDirInfo);
            foreach (var fileInfo in rootDirInfo.EnumerateFiles())
            {
                var file = new FileData(fileInfo);
                folderData.Files.Add(file);
            }

            var tasks = new List<Task<FolderData>>();
            foreach (var dirInfo in rootDirInfo.EnumerateDirectories())
            {
                var task = GetFolderData(dirInfo);
                tasks.Add(task);
            }

            var subFolders = await Task.WhenAll(tasks);
            folderData.SubFolders.AddRange(subFolders);

            return folderData;
        }
    }
}
