using System;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IMonitoringService : IDisposable
    {
        Task<SystemUsage> GetSystemUsageAsync();
        Task<GlobalUsageSnapshot> GetGlobalUsageAsync(System.Threading.CancellationToken cancellationToken = default);
        TimeSpan GetIdleTime();

        /// <summary>Inicia los contadores de rendimiento. Es idempotente.</summary>
        Task StartAsync();

        /// <summary>Detiene y libera los contadores de rendimiento para reducir consumo parásito.</summary>
        Task StopAsync();
    }
}
