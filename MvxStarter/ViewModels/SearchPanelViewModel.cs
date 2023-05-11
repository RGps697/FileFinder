
using MvvmCross.Commands;
using MvxStarter.Core.Models;
using MvxStarter.Core.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.ViewModels
{
    public class SearchPanelViewModel : MvxViewModel
    {
        private string _searchValue;
        private string _targetDirectory = "C:\\";
        private int _progressValue = 0;
        private bool _canSearch = true;
        private bool _isSearchInProgress = false;
        private bool _buttonMainThreadIsChecked = true;
        private bool _buttonSeparateThreadIsChecked;
        private bool _buttonParallelIsChecked;
        private int _concurrentOperationCount = 1;
        private string _consoleOutput;

        private ObservableCollection<FileModel> _foundFiles = new ObservableCollection<FileModel>();
        private ISearchEngine _searchEngine;
        private CancellationTokenSource cts;

        public string SearchValue
        {
            get { return _searchValue; }
            set { _searchValue = value; }
        }
        public string SearchValueArgument
        {
            get { return $"*{_searchValue}*"; }
        }
        public string TargetDirectory
        {
            get
            {
                return _targetDirectory;
            }
            set
            {
                SetProperty(ref _targetDirectory, value);
            }
        }
        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                SetProperty(ref _progressValue, value);
            }
        }
        public bool CanSearch
        {
            get { return _canSearch; }
            private set 
            { 
                _canSearch = value;
                RaisePropertyChanged(() => CanSearch);
            }
        }
        public bool IsSearchInProgress

        {
            get { return _isSearchInProgress; }
            private set 
            { 
                _isSearchInProgress = value;
                RaisePropertyChanged(() => IsSearchInProgress);
            }
        }
        public bool ButtonMainThreadIsChecked
        {
            get { return _buttonMainThreadIsChecked; }
            set { _buttonMainThreadIsChecked = value; }
        }
        public bool ButtonSeparateThreadIsChecked
        {
            get { return _buttonSeparateThreadIsChecked; }
            set { _buttonSeparateThreadIsChecked = value; }
        }
        public bool ButtonParallelIsChecked
        {
            get { return _buttonParallelIsChecked; }
            set 
            {
                _buttonParallelIsChecked = value;
                RaisePropertyChanged(() => ButtonParallelIsChecked);
            }
        }
        public int ConcurrentOperationCount
        {
            get { return _concurrentOperationCount; }
            set 
            {
                if (value >= -1 && value != 0)
                {
                    _concurrentOperationCount = value;
                }
            }
        }

        public string ConsoleOutput {
            get
            {
                return _consoleOutput;
            }

            set
            {
                _consoleOutput = value;
                RaisePropertyChanged(() => ConsoleOutput);
            }
        }
        public ObservableCollection<FileModel> FoundFiles
        {
            get
            {
                return _foundFiles;
            }
            set
            {
                SetProperty(ref _foundFiles, value);
            }
        }
        public ISearchEngine SearchEngine { 
            get
            {
                return _searchEngine;
            }
            private set
            {
                _searchEngine= value;
            }
        }

        public IMvxAsyncCommand FindFilesCommand { get; set; }
        public IMvxCommand StopSearchCommand { get; set; }



        public SearchPanelViewModel()
        {
            SearchEngine = new SearchEngine(WriteInConsole);
            FindFilesCommand = new MvxAsyncCommand(FindFiles);
            StopSearchCommand = new MvxCommand(StopSearch);
        }

        public async Task FindFiles()
        {
            if (!Directory.Exists(TargetDirectory))
            {
                WriteInConsole("Selected directory does not exist");
                return;
            }
            else
            {
                ProgressValue = 0;
                SwitchSearchState();
                FoundFiles.Clear();
                Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
                progress.ProgressChanged += ReportProgress;
                ProgressThrottler progressThrottler = new ProgressThrottler(progress);
                cts = new CancellationTokenSource();
                WriteInConsole($"Search in directory: {TargetDirectory}. File name: {SearchValue}.");

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    if (ButtonMainThreadIsChecked)
                        SearchEngine.FindFiles(TargetDirectory, SearchValueArgument, progressThrottler, cts.Token);

                    else if (ButtonSeparateThreadIsChecked)
                        await Task.Run(() => SearchEngine.FindFiles(TargetDirectory, SearchValueArgument, progressThrottler, cts.Token));

                    else if (ButtonParallelIsChecked)
                        await SearchEngine.FindFilesParallel(TargetDirectory, SearchValueArgument, ConcurrentOperationCount, progressThrottler, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    WriteInConsole("Canceled");
                }

                stopwatch.Stop();
                progressThrottler.FinishReporting();
                WriteInConsole($"Time elapsed: {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
                WriteConsoleSeparator();
                SwitchSearchState();
            }
        }

        public void StopSearch()
        {
            cts.Cancel();
        }

        private void WriteInConsole(string text)
        {
            ConsoleOutput += $">>{text} {Environment.NewLine}";
        }

        private void WriteConsoleSeparator()
        {
            ConsoleOutput += $"{Environment.NewLine}";
        }

        private void ReportProgress(object? sender, ProgressReportModel e)
        {
            for (int i = 0; i < e.FoundFiles.Count; i++)
            {
                WriteFoundFiles(e.FoundFiles[i]);
            }

            if (e.PercentageComplete > ProgressValue)
            {
                ProgressValue = e.PercentageComplete;
            }
        }

        private void WriteFoundFiles(FileModel item)
        {
            FoundFiles.Add(item);
        }

        private void SwitchSearchState()
        {
            CanSearch = !CanSearch;
            IsSearchInProgress = !IsSearchInProgress;
        }
    }
}
