using System.Text;
using SutilBox.CLI.Models;

namespace SutilBox.CLI.Core;

public class UnixDiagnosticEngine : IDiagnosticEngine
{
    public string PlatformName => PlatformDetector.GetPlatformName();

    public async Task<ScanResult> RunScanAsync()
    {
        var issues = new List<ScanIssue>();
        var categorySummary = new Dictionary<ScanCategory, int>();

        // 1. Check disk usage
        issues.AddRange(await CheckDiskUsageAsync());

        // 2. Check memory usage
        issues.AddRange(await CheckMemoryUsageAsync());

        // 3. Check CPU load
        issues.AddRange(await CheckCpuLoadAsync());

        // 4. Check temperature (if available)
        issues.AddRange(await CheckTemperatureAsync());

        // 5. Check disk SMART health (if smartctl available)
        issues.AddRange(await CheckDiskSmartAsync());

        // 6. Check system uptime and load
        issues.AddRange(await CheckSystemLoadAsync());

        // Build category summary
        foreach (var issue in issues)
        {
            if (!categorySummary.ContainsKey(issue.Category))
                categorySummary[issue.Category] = 0;
            categorySummary[issue.Category]++;
        }

        var healthScore = CalculateHealthScore(issues);

        return new ScanResult
        {
            Timestamp = DateTime.Now,
            Issues = issues,
            HealthScore = healthScore,
            CategorySummary = categorySummary
        };
    }

    public async Task<Dictionary<string, string>> GetSystemInfoAsync()
    {
        var info = new Dictionary<string, string>();

        // OS info
        info["Sistema Operativo"] = await RunCommand("uname", "-sr") ?? "Desconocido";
        var prettyName = await RunCommand("lsb_release", "-ds");
        if (string.IsNullOrEmpty(prettyName))
        {
            prettyName = await ReadFileLine("/etc/os-release", line => line.StartsWith("PRETTY_NAME="));
            if (!string.IsNullOrEmpty(prettyName))
                prettyName = prettyName.Replace("PRETTY_NAME=", "").Trim('"');
        }
        if (!string.IsNullOrEmpty(prettyName))
            info["Distribución"] = prettyName.TrimStart();

        // Kernel
        info["Kernel"] = await RunCommand("uname", "-r") ?? "Desconocido";

        // Hostname
        info["Nombre del equipo"] = await RunCommand("hostname", "") ?? "Desconocido";

        // CPU
        var cpuModel = await ReadFileLine("/proc/cpuinfo", line => line.StartsWith("model name"));
        if (!string.IsNullOrEmpty(cpuModel))
            info["CPU"] = cpuModel.Split(':', StringSplitOptions.RemoveEmptyEntries).Last().Trim();
        var cpuCores = await RunCommand("nproc", "");
        if (!string.IsNullOrEmpty(cpuCores))
            info["Núcleos CPU"] = cpuCores.Trim();

        // RAM
        var memTotal = await ReadFileLine("/proc/meminfo", line => line.StartsWith("MemTotal"));
        if (!string.IsNullOrEmpty(memTotal))
        {
            var value = memTotal.Split(':', StringSplitOptions.RemoveEmptyEntries).Last().Trim();
            info["RAM Total"] = value;
        }
        var memAvailable = await ReadFileLine("/proc/meminfo", line => line.StartsWith("MemAvailable"));
        if (!string.IsNullOrEmpty(memAvailable))
        {
            var value = memAvailable.Split(':', StringSplitOptions.RemoveEmptyEntries).Last().Trim();
            info["RAM Disponible"] = value;
        }

        // Disks
        var diskInfo = await RunCommand("lsblk", "-o NAME,SIZE,TYPE,MOUNTPOINT,MODEL");
        if (!string.IsNullOrEmpty(diskInfo))
            info["Discos"] = diskInfo.Trim();

        // Network
        var ipInfo = await RunCommand("ip", "-brief addr");
        if (string.IsNullOrEmpty(ipInfo))
            ipInfo = await RunCommand("ifconfig", "");
        if (!string.IsNullOrEmpty(ipInfo))
            info["Red"] = ipInfo.Trim();

        return info;
    }

    // --- Disk checks ---

    private async Task<List<ScanIssue>> CheckDiskUsageAsync()
    {
        var issues = new List<ScanIssue>();
        var output = await RunCommand("df", "-h");
        if (string.IsNullOrEmpty(output)) return issues;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) continue;

            var usePercent = parts[4].Replace("%", "");
            var mountPoint = parts[1];
            var filesystem = parts[0];

            // Skip virtual filesystems
            if (mountPoint.StartsWith("/proc") || mountPoint.StartsWith("/sys") ||
                filesystem.StartsWith("tmpfs") || filesystem.StartsWith("devtmpfs") ||
                filesystem.StartsWith("overlay"))
                continue;

            if (int.TryParse(usePercent, out var percent))
            {
                if (percent >= 90)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Disco,
                        Severity = IssueSeverity.Critical,
                        Title = $"Disco casi lleno: {mountPoint}",
                        Description = $"El punto de montaje {mountPoint} está al {percent}% de capacidad.",
                        Solution = "Elimina archivos temporales, vacía la papelera, o mueve archivos a otro disco. Ejecuta: sudo apt clean && journalctl --vacuum-size=100M",
                        IsSolvable = true
                    });
                }
                else if (percent >= 75)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Disco,
                        Severity = IssueSeverity.Warning,
                        Title = $"Uso de disco alto: {mountPoint}",
                        Description = $"El punto de montaje {mountPoint} está al {percent}% de capacidad.",
                        Solution = "Considera limpiar archivos temporales o mover datos a otro disco.",
                        IsSolvable = true
                    });
                }
            }
        }

        return issues;
    }

    // --- Memory checks ---

    private async Task<List<ScanIssue>> CheckMemoryUsageAsync()
    {
        var issues = new List<ScanIssue>();

        var memTotal = await ReadFileLine("/proc/meminfo", line => line.StartsWith("MemTotal"));
        var memAvailable = await ReadFileLine("/proc/meminfo", line => line.StartsWith("MemAvailable"));

        if (string.IsNullOrEmpty(memTotal) || string.IsNullOrEmpty(memAvailable)) return issues;

        var totalKb = ParseKb(memTotal);
        var availableKb = ParseKb(memAvailable);
        if (totalKb == 0) return issues;

        var usedPercent = ((totalKb - availableKb) * 100) / totalKb;

        if (usedPercent >= 90)
        {
            issues.Add(new ScanIssue
            {
                Category = ScanCategory.Rendimiento,
                Severity = IssueSeverity.Critical,
                Title = "Memoria RAM casi agotada",
                Description = $"La memoria RAM está al {usedPercent}% de uso. Esto puede causar lentitud extrema o congelamiento del sistema.",
                Solution = "Cierra aplicaciones que consumen mucha memoria. Identifica procesos con: ps aux --sort=-%mem | head -10",
                IsSolvable = true
            });
        }
        else if (usedPercent >= 75)
        {
            issues.Add(new ScanIssue
            {
                Category = ScanCategory.Rendimiento,
                Severity = IssueSeverity.Warning,
                Title = "Uso de memoria RAM alto",
                Description = $"La memoria RAM está al {usedPercent}% de uso.",
                Solution = "Monitorea los procesos que más memoria consumen: ps aux --sort=-%mem | head -10",
                IsSolvable = false
            });
        }

        // Check swap usage
        var swapTotal = await ReadFileLine("/proc/meminfo", line => line.StartsWith("SwapTotal"));
        var swapFree = await ReadFileLine("/proc/meminfo", line => line.StartsWith("SwapFree"));
        if (!string.IsNullOrEmpty(swapTotal) && !string.IsNullOrEmpty(swapFree))
        {
            var swapTotalKb = ParseKb(swapTotal);
            var swapFreeKb = ParseKb(swapFree);
            if (swapTotalKb > 0)
            {
                var swapUsedPercent = ((swapTotalKb - swapFreeKb) * 100) / swapTotalKb;
                if (swapUsedPercent >= 80)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Rendimiento,
                        Severity = IssueSeverity.Warning,
                        Title = "Uso alto de Swap",
                        Description = $"El espacio de intercambio (Swap) está al {swapUsedPercent}% de uso. Esto indica que el sistema no tiene suficiente RAM.",
                        Solution = "Considera agregar más RAM o cerrar aplicaciones que consumen mucha memoria.",
                        IsSolvable = false
                    });
                }
            }
        }

        return issues;
    }

    // --- CPU load checks ---

    private async Task<List<ScanIssue>> CheckCpuLoadAsync()
    {
        var issues = new List<ScanIssue>();

        var loadAvg = await ReadFile("/proc/loadavg");
        if (string.IsNullOrEmpty(loadAvg)) return issues;

        var parts = loadAvg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3) return issues;

        var cores = await RunCommand("nproc", "");
        if (!string.IsNullOrEmpty(cores) && double.TryParse(parts[0], out var load1) && int.TryParse(cores.Trim(), out var numCores))
        {
            // Load average > numCores means processes are waiting for CPU
            var loadRatio = load1 / numCores;

            if (loadRatio >= 4.0)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Rendimiento,
                    Severity = IssueSeverity.Critical,
                    Title = "Carga de CPU muy alta",
                    Description = $"La carga del sistema es {load1:F1} con {numCores} núcleos ({loadRatio:F1}x la capacidad). El sistema está sobrecargado.",
                    Solution = "Identifica los procesos que más CPU consumen: ps aux --sort=-%cpu | head -10",
                    IsSolvable = true
                });
            }
            else if (loadRatio >= 2.0)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Rendimiento,
                    Severity = IssueSeverity.Warning,
                    Title = "Carga de CPU alta",
                    Description = $"La carga del sistema es {load1:F1} con {numCores} núcleos ({loadRatio:F1}x la capacidad).",
                    Solution = "Monitorea los procesos activos: top -bn1 | head -20",
                    IsSolvable = false
                });
            }
        }

        return issues;
    }

    // --- Temperature checks ---

    private async Task<List<ScanIssue>> CheckTemperatureAsync()
    {
        var issues = new List<ScanIssue>();

        // Try /sys/class/thermal/
        var thermalDir = "/sys/class/thermal/";
        if (Directory.Exists(thermalDir))
        {
            var zones = Directory.GetDirectories(thermalDir, "thermal_zone*");
            foreach (var zone in zones)
            {
                var tempFile = Path.Combine(zone, "temp");
                var typeFile = Path.Combine(zone, "type");

                if (File.Exists(tempFile))
                {
                    var tempStr = await File.ReadAllTextAsync(tempFile);
                    if (int.TryParse(tempStr.Trim(), out var tempMilli))
                    {
                        var tempCelsius = tempMilli / 1000.0;
                        var typeName = File.Exists(typeFile) ? await File.ReadAllTextAsync(typeFile) : "Desconocido";
                        typeName = typeName.Trim();

                        if (tempCelsius >= 85)
                        {
                            issues.Add(new ScanIssue
                            {
                                Category = ScanCategory.Hardware,
                                Severity = IssueSeverity.Critical,
                                Title = $"Temperatura crítica: {typeName}",
                                Description = $"La temperatura de {typeName} es de {tempCelsius:F1}°C. Riesgo de daño por sobrecalentamiento.",
                                Solution = "Verifica la ventilación, limpia el equipo de polvo y revisa los ventiladores.",
                                IsSolvable = false
                            });
                        }
                        else if (tempCelsius >= 70)
                        {
                            issues.Add(new ScanIssue
                            {
                                Category = ScanCategory.Hardware,
                                Severity = IssueSeverity.Warning,
                                Title = $"Temperatura alta: {typeName}",
                                Description = $"La temperatura de {typeName} es de {tempCelsius:F1}°C.",
                                Solution = "Monitorea la temperatura con: sensors (requiere lm-sensors instalado).",
                                IsSolvable = false
                            });
                        }
                    }
                }
            }
        }

        // Try lm-sensors
        var sensorsOutput = await RunCommand("sensors", "");
        if (!string.IsNullOrEmpty(sensorsOutput))
        {
            // Parse sensors output for critical temperatures
            foreach (var line in sensorsOutput.Split('\n'))
            {
                if (line.Contains("Crit") || line.Contains("crit"))
                {
                    var parts = line.Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (double.TryParse(part.Trim(), out var temp))
                        {
                            if (temp >= 85)
                            {
                                issues.Add(new ScanIssue
                                {
                                    Category = ScanCategory.Hardware,
                                    Severity = IssueSeverity.Critical,
                                    Title = "Temperatura crítica detectada (sensors)",
                                    Description = $"Se detectó una temperatura de {temp:F1}°C que supera el límite crítico.",
                                    Solution = "Apaga el equipo, verifica ventilación y limpieza interna.",
                                    IsSolvable = false
                                });
                                break;
                            }
                        }
                    }
                }
            }
        }

        return issues;
    }

    // --- Disk SMART checks ---

    private async Task<List<ScanIssue>> CheckDiskSmartAsync()
    {
        var issues = new List<ScanIssue>();

        // Get list of block devices
        var devices = await RunCommand("lsblk", "-dnpo NAME,TYPE");
        if (string.IsNullOrEmpty(devices)) return issues;

        foreach (var line in devices.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[1] == "disk")
            {
                var deviceName = parts[0];
                var devicePath = $"/dev/{deviceName}";

                var smartOutput = await RunCommand("smartctl", $"--health {devicePath}");
                if (!string.IsNullOrEmpty(smartOutput))
                {
                    if (smartOutput.Contains("PASSED", StringComparison.OrdinalIgnoreCase) == false &&
                        smartOutput.Contains("FAILING", StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Disco,
                            Severity = IssueSeverity.Critical,
                            Title = $"Disco con problemas: {deviceName}",
                            Description = $"El disco {deviceName} ha fallado el test SMART de salud. Riesgo de pérdida de datos.",
                            Solution = "Haz backup inmediato de tus datos. Reemplaza el disco lo antes posible.",
                            IsSolvable = false
                        });
                    }
                }
            }
        }

        return issues;
    }

    // --- System load checks ---

    private async Task<List<ScanIssue>> CheckSystemLoadAsync()
    {
        var issues = new List<ScanIssue>();

        // Check uptime
        var uptime = await ReadFile("/proc/uptime");
        if (!string.IsNullOrEmpty(uptime))
        {
            var seconds = double.TryParse(uptime.Split(' ')[0], out var secs) ? secs : 0;
            var hours = seconds / 3600;
            var days = hours / 24;

            if (days > 365)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Sistema,
                    Severity = IssueSeverity.Info,
                    Title = "Sistema sin reinicio por mucho tiempo",
                    Description = $"El sistema lleva {days:F0} días sin reiniciar. Considera reiniciar para aplicar actualizaciones pendientes.",
                    Solution = "Reinicia el sistema cuando sea conveniente: sudo reboot",
                    IsSolvable = true
                });
            }
        }

        return issues;
    }

    // --- Helper methods ---

    private int CalculateHealthScore(List<ScanIssue> issues)
    {
        var score = 100;
        foreach (var issue in issues)
        {
            switch (issue.Severity)
            {
                case IssueSeverity.Critical:
                    score -= 15;
                    break;
                case IssueSeverity.Warning:
                    score -= 5;
                    break;
                case IssueSeverity.Info:
                    score -= 1;
                    break;
            }
        }
        return Math.Max(0, Math.Min(100, score));
    }

    private async Task<string?> RunCommand(string command, string arguments = "", int timeoutSeconds = 10)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null) return null;

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

            var completed = await Task.WhenAny(outputTask, timeoutTask);
            if (completed == timeoutTask)
            {
                try { process.Kill(); } catch { }
                return null;
            }

            if (!process.HasExited)
            {
                try { process.Kill(); } catch { }
                return null;
            }

            return process.ExitCode == 0 ? outputTask.Result : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> ReadFileLine(string path, Func<string, bool> predicate)
    {
        try
        {
            await foreach (var line in File.ReadLinesAsync(path))
            {
                if (predicate(line)) return line;
            }
        }
        catch
        {
            // File not available
        }
        return null;
    }

    private async Task<string?> ReadFile(string path)
    {
        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch
        {
            return null;
        }
    }

    private long ParseKb(string line)
    {
        var value = line.Split(':', StringSplitOptions.RemoveEmptyEntries).Last().Trim();
        // Remove " kB" suffix
        value = value.Replace("kB", "").Trim();
        return long.TryParse(value, out var kb) ? kb : 0;
    }
}
