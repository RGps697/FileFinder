
namespace MvxStarter.Core.Services
{
    public interface ISearchEngine
    {
        public Task<List<FileModel>> FindFile(string searchPath, string fileName, IProgress<ProgressReportModel> progress);
    }
}
