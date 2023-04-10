using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.Models
{
    public class FileModel
    {
        public FileModel(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string? Type { get; set; }
        public string? LastModified { get; set; }
    }
}
