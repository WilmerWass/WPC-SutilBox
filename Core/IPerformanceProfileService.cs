using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IPerformanceProfileService
    {
        Task<ApplyProfileResult> ApplyProfileAsync(PerformanceMode mode);
        Task<ApplyProfileResult> RestoreOriginalStateAsync();
    }
}

