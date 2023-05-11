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

        public void FindFiles(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            ConsoleOutput($"Searching for files synchronously...");
            directoriesSearched = 0;
            GetAllSubdirectories(searchPath, cancellationToken);
            try
            {
                SearchDirectoryRecursive(searchPath, fileName, progress);
                ConsoleOutput($"Directories searched: {directoriesSearched}");
            }
            catch (OperationCanceledException)
            {
                ConsoleOutput("Canceled");
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
            catch (OperationCanceledException) { }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine($"Cannot access directory: {directoryPath}");
            }
            catch (IOException) { }
        }

        private void SearchDirectoryRecursive(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            try
            {
                string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
                List<FileModel>? foundFiles = new List<FileModel>();
                directoriesSearched++;

                if (foundFilesInDirectory.Length > 0)
                {
                    for (int i = 0; i < foundFilesInDirectory.Length; i++)
                    {
                        foundFiles.Add(new FileModel(foundFilesInDirectory[i]));
                    }
                }
                ProgressReportModel report = new ProgressReportModel();
                report.PercentageComplete = directoriesSearched * 100 / directoriesToSearch.Count;
                report.FoundFiles.AddRange(foundFiles);
                progress.Report(report);
                string[] directories = Directory.GetDirectories(directoryPath);
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
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
                Debug.WriteLine($"Cannot access directory: {directoryPath}");
            }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
        }
    }
}
