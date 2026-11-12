using SutilBox.CLI.Models;

namespace SutilBox.CLI.Core;

public interface IDiagnosticEngine
{
    string PlatformName { get; }
    Task<ScanResult> RunScanAsync();
    Task<Dictionary<string, string>> GetSystemInfoAsync();
}
