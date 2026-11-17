using System.Collections.Generic;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public interface IPeripheralDiagnosticService
    {
        Task<IReadOnlyList<PeripheralDiagnostic>> GetDevicesAsync();
        Task<IReadOnlyList<UsbStorageDiagnostic>> GetUsbStorageDevicesAsync();
    }
}
