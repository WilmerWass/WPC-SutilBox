using System.Collections.Generic;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IServiceOptimizerService
    {
        Task<IEnumerable<WindowsService>> GetWindowsServicesAsync();
        Task<bool> StartServiceAsync(string serviceName);
        Task<bool> StopServiceAsync(string serviceName);
        Task<bool> SetServiceStartTypeAsync(string serviceName, ServiceStartType startType);
        
        // Funcionalidad de perfiles (a implementar mÃ¡s tarde)
        // Task<IEnumerable<ServiceProfile>> GetAvailableProfilesAsync();
        // Task<bool> ApplyProfileAsync(ServiceProfile profile);
    }
}

