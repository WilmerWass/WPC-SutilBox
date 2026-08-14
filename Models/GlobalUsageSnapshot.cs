namespace Wpc_SutilBox.Models
{
    /// <summary>
    /// Lectura mínima utilizada por el monitor global del Dashboard.
    /// Los valores anulables distinguen una lectura no disponible de un 0 real.
    /// </summary>
    public sealed class GlobalUsageSnapshot
    {
        public double? CpuUsage { get; init; }
        public double? RamUsage { get; init; }
    }
}
