using System;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IMonitoringService : IDisposable
    {
        Task<SystemUsage> GetSystemUsageAsync();
        TimeSpan GetIdleTime();
    }
}

