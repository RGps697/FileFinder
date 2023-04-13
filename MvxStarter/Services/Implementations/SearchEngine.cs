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

        public List<FileModel> FindFiles(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            List<FileModel> foundFiles = new List<FileModel>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                GetAllSubdirectories(searchPath, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Canceled 2");
            }

            try
            {
                for (int i = 0; i < directoriesToSearch.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    foundFiles.AddRange(SearchDirectory(directoriesToSearch[i], fileName, progress));
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Canceled 3");
            }

            stopwatch.Stop();
            Debug.WriteLine($"Total directory amount: {directoriesToSearch.Count}");
            Debug.WriteLine($"Time elapsed: {stopwatch.Elapsed.Seconds}", ConsoleColor.Green);
            directoriesToSearch.Clear();
            directoriesSearched = 0;
            return foundFiles;
        }

        public async Task FindFilesAsync(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await GetAllSubdirectoriesAsync(searchPath);

            //ParallelOptions parallelOptionsDirectory = new ParallelOptions { MaxDegreeOfParallelism = 1, CancellationToken = cancellationToken };
            //Parallel.ForEach(directoriesToSearch, parallelOptionsDirectory, async (directory) =>
            //{
            //    Debug.WriteLine($"Looking for files in: {directory}");
            //    await SearchDirectory(directory, fileName, progress, cancellationToken);
            //});

            foreach (var d in directoriesToSearch)
            {
                Debug.WriteLine($"directories to search: {d}");
            }
            Debug.WriteLine($"-------------------------------");

            try
            {
                ParallelOptions parallelOptionsFile = new ParallelOptions { MaxDegreeOfParallelism = 1, CancellationToken = cancellationToken};
                Parallel.ForEach(directoriesToSearch, parallelOptionsFile, async (directory) =>
                {
                    Debug.WriteLine($"Looking for files in: {directory}");
                    await SearchDirectoryAsync(directory, fileName, progress, cancellationToken);
                });
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Canceled");
            }

            //SearchDirectory(searchPath, fileName, progress, cancellationToken);



            //tasks.Add(SearchDirectory(searchPath, fileName, progress));
            // Task.Run(() => SearchDirectory(searchPath, fileName, progress));

            stopwatch.Stop();
            Debug.WriteLine($"Total directory amount: {directoriesToSearch.Count}");
            Debug.WriteLine($"Time elapsed: {stopwatch.Elapsed.Seconds}", ConsoleColor.Green);
            directoriesToSearch.Clear();
            directoriesSearched = 0;
        }

        private void GetAllSubdirectories(string directoryPath, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"Current directory: {directoryPath}");
            List<string> subdirectories = new List<string>();

            cancellationToken.ThrowIfCancellationRequested();

            subdirectories.AddRange(Directory.GetDirectories(directoryPath));
            for (int i = 0; i < subdirectories.Count; i++)
            {
                int currentIndex = i;
                Debug.WriteLine($"Getting directory: {subdirectories[currentIndex]}");
                GetAllSubdirectories(subdirectories[i], cancellationToken);
            }

            directoriesToSearch.AddRange(subdirectories);
        }

        private async Task GetAllSubdirectoriesAsync(string directoryPath)
        {
            Debug.WriteLine($"Current directory: {directoryPath}");
            List<string> subdirectories = new List<string>();

            subdirectories.AddRange(Directory.GetDirectories(directoryPath));
            for (int i = 0; i < subdirectories.Count; i++)
            {
                int currentIndex = i;
                Debug.WriteLine($"Getting directory: {subdirectories[currentIndex]}");
                Task.FromResult(GetAllSubdirectoriesAsync(subdirectories[i]));
            }

            directoriesToSearch.AddRange(subdirectories);
        }

        private FileModel[] SearchDirectory(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            Debug.WriteLine($"Thread: {Environment.CurrentManagedThreadId}");
            Debug.WriteLine($"Path: {directoryPath} !");
            string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
            FileModel[]? foundFiles = new FileModel[foundFilesInDirectory.Length];
            directoriesSearched++;
            ProgressReportModel report = new ProgressReportModel();
            if (foundFilesInDirectory.Length > 0)
            {
                for (int i = 0; i < foundFilesInDirectory.Length; i++)
                {
                    Debug.WriteLine($"Found file in directory {directoryPath}: {foundFilesInDirectory[i]}");
                    foundFiles[i] = new FileModel(foundFilesInDirectory[i]);
                }
                
            }
            report.PercentageComplete = directoriesSearched * 100 / directoriesToSearch.Count;
            report.FoundFiles.AddRange(foundFiles);
            progress.Report(report);
            return foundFiles;
        }

        private async Task SearchDirectoryAsync(string directoryPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"Thread: {Environment.CurrentManagedThreadId}");
            Debug.WriteLine($"Path: {directoryPath} !");
            string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
            FileModel[]? foundFiles = new FileModel[foundFilesInDirectory.Length];
            if (foundFilesInDirectory.Length > 0)
            {
                ProgressReportModel report = new ProgressReportModel();
                for(int i = 0; i < foundFilesInDirectory.Length; i++)
                {
                    Debug.WriteLine($"Found file in directory {directoryPath}: {foundFilesInDirectory[i]}");
                    foundFiles[i] = new FileModel(foundFilesInDirectory[i]);
                }
                report.FoundFiles.AddRange(foundFiles);
                progress.Report(report);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
