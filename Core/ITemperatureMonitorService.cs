using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public interface ITemperatureMonitorService
    {
        Task<double?> GetCpuTemperatureCAsync();
    }
}
