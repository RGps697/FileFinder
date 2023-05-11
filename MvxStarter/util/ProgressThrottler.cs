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
        private int _lastItem = 0;

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
                Debug.WriteLine("reporting..");

                ProgressReportModel report = new ProgressReportModel()
                {
                    PercentageComplete = value.PercentageComplete
                };
                report.FoundFiles.AddRange(ReportNewest());
                _progress.Report(report);
            }
        }

        public void FinishReporting()
        {
            _stopwatch.Stop();
            Debug.WriteLine("finishing reporting...");
            ProgressReportModel report = new ProgressReportModel()
            {
                PercentageComplete = 100,
                FoundFiles = ReportNewest()
            };
            _progress.Report(report);
        }

        private List<FileModel> ReportNewest()
        {
            int startingIndex = _lastItem;
            _lastItem = _filesToReport.Count;
            int lastIndex = _lastItem;
            Debug.WriteLine($"Reporting items from: {startingIndex}, count: {lastIndex-startingIndex}, last: {lastIndex}");
            return _filesToReport.GetRange(startingIndex, lastIndex - startingIndex);
        }
    }
}
