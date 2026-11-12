using SutilBox.CLI.Models;

namespace SutilBox.CLI.Core;

// On non-Windows platforms, this is a stub that reports the engine is unavailable.
// On Windows, the CLI can use the WPF project services directly.
public class WindowsDiagnosticEngine : IDiagnosticEngine
{
    public string PlatformName => "Windows (no disponible)";

    public Task<ScanResult> RunScanAsync()
    {
        return Task.FromResult(new ScanResult
        {
            Timestamp = DateTime.Now,
            Issues = new List<ScanIssue>
            {
                new ScanIssue
                {
                    Category = ScanCategory.Sistema,
                    Severity = IssueSeverity.Critical,
                    Title = "Motor de Windows no disponible",
                    Description = "Este motor solo funciona en Windows. Usa el motor Unix para Linux/macOS.",
                    Solution = "Ejecuta este CLI en un sistema Windows para usar el motor completo.",
                    IsSolvable = false
                }
            },
            HealthScore = 0,
            CategorySummary = new Dictionary<ScanCategory, int>()
        });
    }

    public Task<Dictionary<string, string>> GetSystemInfoAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            ["Plataforma"] = "Windows (no disponible en este sistema)"
        });
    }
}
