using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.util
{
    public class ProgressThrottler : IProgress<ProgressReportModel>
    {
        private readonly IProgress<ProgressReportModel> _progress;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private List<FileModel> _filesToReport = new List<FileModel>();

        public ProgressThrottler(IProgress<ProgressReportModel> progress)
        {
            _progress = progress ?? throw new ArgumentNullException("Passed null progress argument");
        }

        public void Report(ProgressReportModel value)
        {
            _filesToReport.AddRange(value.FoundFiles);
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                _progress.Report(value);
            }
            else if (_stopwatch.ElapsedMilliseconds > 500)
            {
                _stopwatch.Restart();
                ProgressReportModel report = new ProgressReportModel()
                {
                    PercentageComplete = value.PercentageComplete
                };
                for (int i = 0; i < _filesToReport.Count; i++)
                {
                    report.FoundFiles.Add(new FileModel(_filesToReport[i].Name));
                }
                _progress.Report(report);
                _filesToReport.Clear();
            }
        }

        public void FinishReporting()
        {
            _stopwatch.Stop();
            ProgressReportModel report = new ProgressReportModel()
            {
                PercentageComplete = 100,
                FoundFiles = _filesToReport
            };
            _progress.Report(report);
        }
    }
}
