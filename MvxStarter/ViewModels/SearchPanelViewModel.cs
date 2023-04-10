
using MvvmCross.Commands;
using MvxStarter.Core.Models;
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
        private ObservableCollection<FileModel> _foundFiles = new ObservableCollection<FileModel>();
        private ISearchEngine _searchEngine = new SearchEngine();

        public string SearchValue
        {
            get { return _searchValue; }
            set { SetProperty(ref _searchValue, value); }
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
        public IMvxCommand SelectDirectoryCommand { get; set; }

        public SearchPanelViewModel()
        {
            FindFilesCommand = new MvxAsyncCommand(FindFiles);
        }

        public async Task FindFiles()
        {
            FoundFiles.Clear();
            Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += ReportProgress;
            Debug.WriteLine($"Search in: {TargetDirectory}, file name: {SearchValue}");
            SearchEngine.FindFile(TargetDirectory, SearchValue, progress);
        }

        private void ReportProgress(object? sender, ProgressReportModel e)
        {
            Debug.WriteLine($"Reported");
            for (int i = 0; i < e.FoundFiles.Count; i++)
            {
                Debug.WriteLine(i + ": " + e.FoundFiles[i]);
            }
            for (int i = 0; i < e.FoundFiles.Count; i++)
            {
                FoundFiles.Add(e.FoundFiles[i]);
            }
            for (int i = 0; i < FoundFiles.Count; i++)
            {
                Debug.WriteLine("added file: " + FoundFiles[i].Name);
            }

        }
    }
}
