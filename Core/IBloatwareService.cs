using System.Collections.Generic;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IBloatwareService
    {
        Task<IEnumerable<BloatwareApp>> GetBloatwareAppsAsync();
        Task<bool> UninstallBloatwareAppAsync(BloatwareApp app);
    }
}

