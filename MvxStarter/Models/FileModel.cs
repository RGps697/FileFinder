using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.Models
{
    public class FileModel
    {
        public FileModel(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
            Type = fileInfo.Extension;
            LastModified = fileInfo.LastWriteTime.ToString();
        }

        public string Name { get; set; }
        public string? Type { get; set; }
        public string? LastModified { get; set; }
    }
}
