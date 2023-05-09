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
        Action<string> ConsoleOutput { get; set; }

        public SearchEngine()
        {
        }

        public SearchEngine(Action<string> writeInConsole)
        {
            ConsoleOutput = writeInConsole;
        }

        public List<FileModel>? FindFiles(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            ConsoleOutput($"Searching for files synchronously...");
            directoriesSearched = 0;
            GetAllSubdirectories(searchPath, cancellationToken);
            try
            {
                List<FileModel> files = SearchDirectoryRecursive(searchPath, fileName, progress);
                ConsoleOutput($"Directories searched: {directoriesSearched}");
                return files;
            }
            catch (OperationCanceledException)
            {
                ConsoleOutput("Canceled");
                return null;
            }
        }

        public async Task FindFilesParallel(string searchPath, string fileName, int concurrentOperationAmount, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            ConsoleOutput("Searching for files in parallel...");
            await Task.Run(() =>
            {
                GetAllSubdirectories(searchPath, cancellationToken);

                try
                {
                    ParallelOptions parallelOptionsFile = new ParallelOptions { MaxDegreeOfParallelism = concurrentOperationAmount, CancellationToken = cancellationToken };
                    Parallel.ForEach(directoriesToSearch, parallelOptionsFile, (directory) =>
                    {
                        SearchDirectory(directory, fileName, progress);
                    });
                }
                catch (OperationCanceledException)
                {
                    ConsoleOutput("Canceled");
                }
                ConsoleOutput($"Directories searched: {directoriesToSearch.Count}");
                directoriesToSearch.Clear();
                directoriesSearched = 0;
            });
        }

        //PRIVATE METHODS

        private void GetAllSubdirectories(string directoryPath, CancellationToken cancellationToken)
        {
            try
            {
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
            catch (OperationCanceledException)
            {

            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        private async Task GetAllSubdirectoriesAsync(string directoryPath, CancellationToken cancellationToken)
        {
            List<string> subdirectories = new List<string>();

            cancellationToken.ThrowIfCancellationRequested();

            subdirectories.AddRange(Directory.GetDirectories(directoryPath));
            for (int i = 0; i < subdirectories.Count; i++)
            {
                int currentIndex = i;
                GetAllSubdirectoriesAsync(subdirectories[i], cancellationToken);
            }

            directoriesToSearch.AddRange(subdirectories);
        }

        private List<FileModel> SearchDirectoryRecursive(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            try
            {
                string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
                List<FileModel>? foundFiles = new List<FileModel>();
                directoriesSearched++;

                if (foundFilesInDirectory.Length > 0)
                {
                    ProgressReportModel report = new ProgressReportModel();
                    for (int i = 0; i < foundFilesInDirectory.Length; i++)
                    {
                        foundFiles.Add(new FileModel(foundFilesInDirectory[i]));
                    }
                    report.PercentageComplete = directoriesSearched * 100 / directoriesToSearch.Count;
                    report.FoundFiles.AddRange(foundFiles);
                    progress.Report(report);
                }
                string[] directories = Directory.GetDirectories(directoryPath);
                for (int i = 0; i < directories.Length; i++)
                {
                    foundFiles.AddRange(SearchDirectoryRecursive(directories[i], fileName, progress));
                }
                return foundFiles;
            }
            catch (UnauthorizedAccessException)
            {
                return new List<FileModel>();
            }
            catch (DirectoryNotFoundException)
            {
                return new List<FileModel>();
            }
        }

        private void SearchDirectory(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            try { 
                string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
                directoriesSearched++;
                FileModel[]? foundFiles = new FileModel[foundFilesInDirectory.Length];
                if (foundFilesInDirectory.Length > 0)
                {
                    for (int i = 0; i < foundFilesInDirectory.Length; i++)
                    {
                        foundFiles[i] = new FileModel(foundFilesInDirectory[i]);
                    }

                }
                ProgressReportModel report = new ProgressReportModel();
                report.PercentageComplete = directoriesSearched * 100 / directoriesToSearch.Count;
                report.FoundFiles.AddRange(foundFiles);
                progress.Report(report);
            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (DirectoryNotFoundException)
            {

            }
        }
    }
}
