using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IDuplicateFileService
    {
        Task<DuplicateScanResult> FindDuplicatesAsync(
            string rootPath,
            IProgress<string>? progress,
            CancellationToken cancellationToken);
    }
}
