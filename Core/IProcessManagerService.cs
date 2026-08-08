using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IProcessManagerService
    {
        Task<IEnumerable<ProcessInfoDto>> GetProcessesAsync();
        Task<bool> SetPriorityAsync(int pid, ProcessPriorityClass priority);
        Task<bool> KillProcessAsync(int pid);
        Task<ProcessImpactStats> ComputeImpactAsync();
        Task<int> ReduceBackgroundProcessesAsync(ProcessPriorityClass targetPriority);
        Task OptimizeRamAsync();
    }
}


