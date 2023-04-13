
namespace MvxStarter.Core.Services
{
    public interface ISearchEngine
    {
        public List<FileModel> FindFiles(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken);
        public Task FindFilesAsync(string searchPath, string fileName, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken);
    }
}
