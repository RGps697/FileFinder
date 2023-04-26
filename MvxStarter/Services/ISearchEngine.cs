
namespace MvxStarter.Core.Services
{
    public interface ISearchEngine
    {
        public List<FileModel>? FindFiles(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken);
        public Task FindFilesParallel(string searchPath, string fileName, int ConcurrentOperationCount, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken);
    }
}
