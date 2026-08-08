using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public interface ISystemInfoService
    {
        Task<SystemInfo> GetSystemInfoAsync();
    }
}

