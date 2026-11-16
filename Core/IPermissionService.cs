using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public interface IPermissionService
    {
        bool IsAdministrator { get; }
        Task<bool> IsElevatedAsync();
        Task<(bool Success, string Message)> RequireElevationAsync();
        Task<bool> CanModifyRegistryAsync();
        Task<bool> CanManageServicesAsync();
        Task<bool> CanAccessWmiAsync();
    }
}
