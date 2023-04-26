using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.Services.Implementations
{
    public class SearchEngine : ISearchEngine
    {
        List<string> directoriesToSearch = new List<string>();
        int directoriesSearched = 0;

        public SearchEngine()
        {
        }

        public List<FileModel>? FindFiles(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            directoriesSearched = 0;
            GetAllSubdirectories(searchPath, cancellationToken);
            try
            {
                List<FileModel> files = SearchDirectory(searchPath, fileName, progress);
                Debug.WriteLine($"Directories searched: {directoriesSearched}");
                return files;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Canceled");
                return null;
            }
        }

        public async Task FindFilesParallel(string searchPath, string fileName, int concurrentOperationAmount, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            GetAllSubdirectories(searchPath, cancellationToken);

            foreach (var d in directoriesToSearch)
            {
                Debug.WriteLine($"directories to search: {d}");
            }
            Debug.WriteLine($"-------------------------------");

            try
            {
                Debug.WriteLine($"OPERATIONS: {concurrentOperationAmount}");
                ParallelOptions parallelOptionsFile = new ParallelOptions { MaxDegreeOfParallelism = concurrentOperationAmount, CancellationToken = cancellationToken};
                Parallel.ForEach(directoriesToSearch, parallelOptionsFile, async (directory) =>
                {
                    Debug.WriteLine($"Looking for files in: {directory}");
                    SearchDirectoryAsync(directory, fileName, progress);
                });
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Canceled");
            }

            //SearchDirectory(searchPath, fileName, progress, cancellationToken);

            //tasks.Add(SearchDirectory(searchPath, fileName, progress));
            // Task.Run(() => SearchDirectory(searchPath, fileName, progress));
            Debug.WriteLine($"Total directory amount: {directoriesToSearch.Count}");
            directoriesToSearch.Clear();
            directoriesSearched = 0;
        }

        //PRIVATE METHODS

        private void GetAllSubdirectories(string directoryPath, CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine($"Current directory: {directoryPath}");
                List<string> subdirectories = new List<string>();

                cancellationToken.ThrowIfCancellationRequested();

                subdirectories.AddRange(Directory.GetDirectories(directoryPath));
                for (int i = 0; i < subdirectories.Count; i++)
                {
                    int currentIndex = i;
                    GetAllSubdirectories(subdirectories[i], cancellationToken);
                }

                directoriesToSearch.AddRange(subdirectories);
            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        private async Task GetAllSubdirectoriesAsync(string directoryPath, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"Current directory: {directoryPath}");
            List<string> subdirectories = new List<string>();

            cancellationToken.ThrowIfCancellationRequested();

            subdirectories.AddRange(Directory.GetDirectories(directoryPath));
            for (int i = 0; i < subdirectories.Count; i++)
            {
                int currentIndex = i;
                Debug.WriteLine($"Getting directory: {subdirectories[currentIndex]}");
                GetAllSubdirectoriesAsync(subdirectories[i], cancellationToken);
            }

            directoriesToSearch.AddRange(subdirectories);
        }

        private List<FileModel> SearchDirectory(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            try
            {
                string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
                List<FileModel>? foundFiles = new List<FileModel>();
                directoriesSearched++;
                Debug.WriteLine($"Length: {foundFilesInDirectory.Length}");

                if (foundFilesInDirectory.Length > 0)
                {
                    ProgressReportModel report = new ProgressReportModel();
                    for (int i = 0; i < foundFilesInDirectory.Length; i++)
                    {
                        Debug.WriteLine($"i: {i}");
                        foundFiles.Add(new FileModel(foundFilesInDirectory[i]));
                    }
                    report.PercentageComplete = directoriesSearched * 100 / directoriesToSearch.Count;
                    report.FoundFiles.AddRange(foundFiles);
                    progress.Report(report);
                }
                string[] directories = Directory.GetDirectories(directoryPath);
                for (int i = 0; i < directories.Length; i++)
                {
                    foundFiles.AddRange(SearchDirectory(directories[i], fileName, progress));
                }
                return foundFiles;
            }
            catch (UnauthorizedAccessException)
            {
                return new List<FileModel>();
            }
        }

        private async Task SearchDirectoryAsync(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            Debug.WriteLine($"Thread: {Environment.CurrentManagedThreadId}");
            Debug.WriteLine($"Path: {directoryPath} !");
            string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
            FileModel[]? foundFiles = new FileModel[foundFilesInDirectory.Length];
            if (foundFilesInDirectory.Length > 0)
            {
                ProgressReportModel report = new ProgressReportModel();
                for (int i = 0; i < foundFilesInDirectory.Length; i++)
                {
                    Debug.WriteLine($"Found file in directory {directoryPath}: {foundFilesInDirectory[i]}");
                    foundFiles[i] = new FileModel(foundFilesInDirectory[i]);
                }
                report.FoundFiles.AddRange(foundFiles);
                progress.Report(report);
            }
        }
    }
}
