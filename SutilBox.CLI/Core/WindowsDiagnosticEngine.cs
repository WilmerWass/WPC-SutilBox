using System.Runtime.InteropServices;
using SutilBox.CLI.Models;

namespace SutilBox.CLI.Core;

/// <summary>
/// Read-only Windows diagnostics implemented with APIs available to the .NET 8 CLI.
/// This intentionally does not claim SMART, driver, security, or temperature coverage.
/// </summary>
public sealed class WindowsDiagnosticEngine : IDiagnosticEngine
{
    public string PlatformName => "Windows (diagnóstico básico)";

    public Task<ScanResult> RunScanAsync()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("El motor Windows solo puede ejecutarse en Windows.");

        var issues = new List<ScanIssue>();
        var checksPerformed = new HashSet<ScanCategory>();

        try
        {
            foreach (var drive in DriveInfo.GetDrives().Where(drive => drive.IsReady))
            {
                if (drive.TotalSize <= 0) continue;
                checksPerformed.Add(ScanCategory.Disco);
                var usedPercent = (drive.TotalSize - drive.AvailableFreeSpace) * 100d / drive.TotalSize;
                if (usedPercent >= 95)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Disco,
                        Severity = IssueSeverity.Critical,
                        Title = $"Poco espacio disponible en {drive.Name}",
                        Description = $"La unidad tiene {usedPercent:F0}% de uso.",
                        Solution = "Revisa el almacenamiento y elimina solo archivos que hayas seleccionado.",
                        IsSolvable = false
                    });
                }
                else if (usedPercent >= 85)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Disco,
                        Severity = IssueSeverity.Warning,
                        Title = $"Espacio bajo en {drive.Name}",
                        Description = $"La unidad tiene {usedPercent:F0}% de uso.",
                        Solution = "Revisa qué ocupa espacio antes de eliminar o mover archivos.",
                        IsSolvable = false
                    });
                }
            }
        }
        catch
        {
            // A drive can become unavailable while it is being inspected.
        }

        if (TryGetMemoryStatus(out var memory))
        {
            checksPerformed.Add(ScanCategory.Rendimiento);
            var usedPercent = memory.TotalPhysical == 0
                ? 0
                : (memory.TotalPhysical - memory.AvailablePhysical) * 100d / memory.TotalPhysical;
            if (usedPercent >= 95)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Rendimiento,
                    Severity = IssueSeverity.Critical,
                    Title = "Uso de memoria muy alto",
                    Description = $"La memoria física está al {usedPercent:F0}% de uso.",
                    Solution = "Revisa las aplicaciones con mayor consumo antes de cerrar ninguna.",
                    IsSolvable = false
                });
            }
            else if (usedPercent >= 85)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Rendimiento,
                    Severity = IssueSeverity.Warning,
                    Title = "Uso de memoria elevado",
                    Description = $"La memoria física está al {usedPercent:F0}% de uso.",
                    Solution = "Revisa las aplicaciones y el consumo actual de memoria.",
                    IsSolvable = false
                });
            }
        }

        var summary = issues.GroupBy(issue => issue.Category)
            .ToDictionary(group => group.Key, group => group.Count());
        var score = 100;
        foreach (var issue in issues)
        {
            score -= issue.Severity switch
            {
                IssueSeverity.Critical => 15,
                IssueSeverity.Warning => 5,
                _ => 1
            };
        }

        if (checksPerformed.Count == 0)
        {
            issues.Add(new ScanIssue
            {
                Category = ScanCategory.Sistema,
                Severity = IssueSeverity.Info,
                Title = "No hay datos de diagnóstico disponibles",
                Description = "No se pudieron leer las unidades ni el estado de memoria.",
                Solution = "Vuelve a intentarlo cuando Windows y las unidades estén disponibles.",
                IsSolvable = false
            });
        }

        return Task.FromResult(new ScanResult
        {
            Timestamp = DateTime.Now,
            Issues = issues,
            HealthScore = Math.Clamp(score, 0, 100),
            CategorySummary = issues.GroupBy(issue => issue.Category)
                .ToDictionary(group => group.Key, group => group.Count())
        });
    }

    public Task<Dictionary<string, string>> GetSystemInfoAsync()
    {
        var info = new Dictionary<string, string>();
        if (!OperatingSystem.IsWindows())
        {
            info["Plataforma"] = "El motor de Windows solo puede ejecutarse en Windows.";
            return Task.FromResult(info);
        }

        info["Sistema Operativo"] = Environment.OSVersion.ToString();
        info["Nombre del equipo"] = Environment.MachineName;
        info["Arquitectura"] = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        info["CPU"] = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "No disponible";
        info["Núcleos lógicos"] = Environment.ProcessorCount.ToString();
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        info["Tiempo activo"] = $"{uptime.Days} d {uptime.Hours} h {uptime.Minutes} min";

        if (TryGetMemoryStatus(out var memory))
        {
            info["RAM total"] = FormatBytes(memory.TotalPhysical);
            info["RAM disponible"] = FormatBytes(memory.AvailablePhysical);
        }

        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady)
                .Select(drive => $"{drive.Name} {FormatBytes(drive.AvailableFreeSpace)} libres de {FormatBytes(drive.TotalSize)}");
            info["Unidades"] = string.Join(Environment.NewLine, drives);
        }
        catch
        {
            info["Unidades"] = "No disponibles";
        }

        return Task.FromResult(info);
    }

    private static string FormatBytes(ulong bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return $"{value:0.##} {units[unit]}";
    }

    private static string FormatBytes(long bytes) => FormatBytes((ulong)Math.Max(0, bytes));

    private static bool TryGetMemoryStatus(out MemoryStatus status)
    {
        status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return GlobalMemoryStatusEx(ref status);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus buffer);
}
