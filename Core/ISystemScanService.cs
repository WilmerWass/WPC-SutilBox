using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface ISystemScanService
    {
        Task<ScanResult> RunFullScanAsync(CancellationToken cancellationToken = default);
    }
}
