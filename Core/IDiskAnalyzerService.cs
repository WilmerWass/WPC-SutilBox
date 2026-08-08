using System.Collections.Generic;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IDiskAnalyzerService
    {
        Task<IEnumerable<FolderSizeInfo>> AnalyzeDirectoryAsync(string path);
        Task<IEnumerable<FolderSizeInfo>> FindLargeFilesAsync(string path, long minSizeInBytes);

    }
}

