using System.Collections.Generic;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IDiskHealthService
    {
        Task<IEnumerable<DiskHealthInfo>> GetDiskHealthAsync();
    }
}
