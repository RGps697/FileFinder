using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvxStarter.Core.Services.Implementations
{
    public class SearchEngine : ISearchEngine
    {
        public SearchEngine()
        {
        }

        public async Task<List<FileModel>> FindFile(string searchPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            Task.Run(() => SearchDirectory(searchPath, fileName, progress));
            return null;
        }

        private async Task<FileModel[]> SearchDirectory(string directoryPath, string fileName, IProgress<ProgressReportModel> progress)
        {
            string[] foundFilesInDirectory = Directory.GetFiles($"{directoryPath}", $"{fileName}");
            string[] directories = Directory.GetDirectories(directoryPath);
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
            for (int i = 0; i < directories.Length; i++)
            {
                int currentIndex = i;
                Debug.WriteLine($"Looking for files in: {directories[currentIndex]}, {currentIndex}, {directories.Length}");
                Task.Run(() => SearchDirectory(directories[currentIndex], fileName, progress));
            }
            return foundFiles;
        }

    }
}
