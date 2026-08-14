using System;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IMonitoringService : IDisposable
    {
        Task<SystemUsage> GetSystemUsageAsync();
        Task<GlobalUsageSnapshot> GetGlobalUsageAsync(System.Threading.CancellationToken cancellationToken = default);
        TimeSpan GetIdleTime();
    }
}

