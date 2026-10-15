using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface ISystemMaintenanceService
    {
        Task<CleanResult> CleanTemporaryFilesAsync(CleaningOptions options);
        Task<IReadOnlyList<CleanupCandidate>> PreviewTemporaryFilesAsync(CleaningOptions options, CancellationToken cancellationToken = default);
        Task<CleanResult> CleanTemporaryFilesAsync(IReadOnlyCollection<CleanupCandidate> candidates, CancellationToken cancellationToken = default);
        Task<CleanResult> OptimizeMemoryAsync();
        bool IsAdministrator();
        Task<ProcessLaunchResult> LaunchSystemFileCheckerAsync();
        Task<ProcessLaunchResult> LaunchDISMHealthRestoreAsync();
        Task<ProcessLaunchResult> LaunchCHKDSKAsync();
        Task<ProcessLaunchResult> FlushDnsAsync();
        Task<ProcessLaunchResult> AnalyzeDiskAsync();
        Task<ProcessLaunchResult> CleanPrefetchAsync();
        Task<ProcessLaunchResult> RebuildSearchIndexAsync();
        Task<ProcessLaunchResult> ResetNetworkAsync();
        Task OptimizeSelfAsync();
    }
}

