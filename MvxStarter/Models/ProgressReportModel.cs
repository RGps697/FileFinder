using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.Models
{
    public class ProgressReportModel
    {
        public int PercentageComplete { get; set; }
        public List<FileModel> FoundFiles { get; set; } = new List<FileModel>();
    }
}
