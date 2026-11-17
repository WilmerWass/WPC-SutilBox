namespace Wpc_SutilBox.Models
{
    public sealed class PeripheralDiagnostic
    {
        public string Category { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Manufacturer { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string DeviceId { get; init; } = string.Empty;
    }

    public sealed class UsbStorageDiagnostic
    {
        public string Model { get; init; } = string.Empty;
        public string Capacity { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string DeviceId { get; init; } = string.Empty;
    }
}
