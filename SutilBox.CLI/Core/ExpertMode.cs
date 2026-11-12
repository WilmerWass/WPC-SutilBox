using System.Diagnostics;
using SutilBox.CLI.Models;

namespace SutilBox.CLI.Core;

public static class ExpertMode
{
    public static async Task InteractiveLoop(ScanResult result, IDiagnosticEngine engine)
    {
        var solvableIssues = result.Issues.Where(i => i.IsSolvable && !i.IsResolved).ToList();

        if (solvableIssues.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No hay problemas solucionables en este momento.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║           🎓 MODO EXPERTO INTERACTIVO            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"Se encontraron {solvableIssues.Count} problema(s) que se pueden solucionar.");
        Console.WriteLine();

        while (true)
        {
            // Show menu
            Console.WriteLine("┌─ PROBLEMAS SOLUCIONABLES ─────────────────────────┐");
            for (var idx = 0; idx < solvableIssues.Count; idx++)
            {
                var item = solvableIssues[idx];
                var status = item.IsResolved ? "✅ Resuelto" : "⏳ Pendiente";
                var severityIcon = item.Severity switch
                {
                    IssueSeverity.Critical => "🔴",
                    IssueSeverity.Warning => "🟡",
                    _ => "ℹ️"
                };

                Console.WriteLine($"  {severityIcon} [{idx + 1}] {item.Title} — {status}");
            }
            Console.WriteLine("  ⬅️  [0] Volver");
            Console.WriteLine("  🚪 [s] Salir");
            Console.WriteLine("└────────────────────────────────────────────────────┘");
            Console.WriteLine();

            Console.Write("Elige un número para solucionar: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) continue;

            if (input.ToLower() == "s") break;
            if (input == "0") continue;

            if (!int.TryParse(input, out var choice) || choice < 1 || choice > solvableIssues.Count)
            {
                Console.WriteLine("Opción inválida. Intenta de nuevo.");
                Console.WriteLine();
                continue;
            }

            var selectedIssue = solvableIssues[choice - 1];

            if (selectedIssue.IsResolved)
            {
                Console.WriteLine("Este problema ya fue resuelto.");
                Console.WriteLine();
                continue;
            }

            // Show detailed solution
            await ShowSolution(selectedIssue, engine);

            // Ask if user wants to execute
            Console.WriteLine();
            Console.Write("¿Deseas ejecutar esta solución? (s/n): ");
            var execute = Console.ReadLine()?.Trim().ToLower();

            if (execute == "s" || execute == "si" || execute == "y" || execute == "yes")
            {
                // Capture pre-solution metrics for comparison
                double? preLoadAverage = null;
                if (selectedIssue.Title.Contains("CPU") || selectedIssue.Title.Contains("carga"))
                {
                    preLoadAverage = await GetLoadAverage();
                }

                await ExecuteSolution(selectedIssue, engine);
                selectedIssue.IsResolved = true;
                result.Issues.First(x => x.Title == selectedIssue.Title).IsResolved = true;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("✅ Problema marcado como resuelto.");
                Console.ResetColor();

                // Show post-solution comparison if we have pre-metrics
                if (preLoadAverage.HasValue)
                {
                    await Task.Delay(2000); // Wait 2 seconds for metrics to settle
                    var postLoadAverage = await GetLoadAverage();
                    Console.WriteLine();
                    Console.WriteLine("  ┌─ VERIFICACIÓN POST-SOLUCIÓN ────────────────────┐");
                    Console.WriteLine($"  │ Carga de CPU antes: {preLoadAverage.Value:F1}");
                    Console.WriteLine($"  │ Carga de CPU ahora: {postLoadAverage:F1}");
                    if (postLoadAverage < preLoadAverage.Value)
                    {
                        var improvement = ((preLoadAverage.Value - postLoadAverage) / preLoadAverage.Value * 100);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  │ ✅ Mejora del {improvement:F0}% — La carga ha disminuido");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  │ ⚠️  La carga no ha cambiado significativamente");
                        Console.WriteLine("  │    Es posible que necesites reiniciar el sistema");
                        Console.ResetColor();
                    }
                    Console.WriteLine("  └──────────────────────────────────────────────────┘");
                }
            }

            Console.WriteLine();
            Console.Write("Presiona Enter para continuar...");
            Console.ReadLine();
            Console.WriteLine();

            // Clear screen
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║           🎓 MODO EXPERTO INTERACTIVO            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.WriteLine();

            var remaining = solvableIssues.Count(i => !i.IsResolved);
            if (remaining == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🎉 ¡Felicidades! Has resuelto todos los problemas solucionables.");
                Console.ResetColor();
                break;
            }

            Console.WriteLine($"Quedan {remaining} problema(s) por solucionar.");
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("Gracias por usar SutilBox CLI. ¡Tu sistema está más saludable! 🚀");
    }

    static async Task ShowSolution(ScanIssue issue, IDiagnosticEngine engine)
    {
        Console.WriteLine();
        Console.WriteLine("┌────────────────────────────────────────────────────┐");
        Console.WriteLine($"│  🔍 {issue.Title}");
        Console.WriteLine("└────────────────────────────────────────────────────┘");
        Console.WriteLine();
        Console.WriteLine($"Problema: {issue.Description}");
        Console.WriteLine();

        if (!string.IsNullOrEmpty(issue.Solution))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("💊 Solución recomendada:");
            Console.ResetColor();
            Console.WriteLine();

            // Check if solution contains commands
            var solutionLines = issue.Solution.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in solutionLines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("sudo ") || trimmed.StartsWith("apt ") ||
                    trimmed.StartsWith("rm ") || trimmed.StartsWith("journalctl") ||
                    trimmed.StartsWith("ps ") || trimmed.StartsWith("top ") ||
                    trimmed.StartsWith("reboot") || trimmed.StartsWith("shutdown"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  $ {trimmed}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {trimmed}");
                }
            }
        }
        else
        {
            Console.WriteLine("No hay una solución automática disponible para este problema.");
        }

        // Show platform-specific tips
        if (PlatformDetector.IsLinux)
        {
            Console.WriteLine();
            Console.WriteLine("📌 Consejos adicionales para Linux:");
            Console.WriteLine("  - Algunos comandos requieren permisos de root (sudo)");
            Console.WriteLine("  - Revisa los logs con: journalctl -xe");
            Console.WriteLine("  - Monitorea el sistema con: htop o top");
        }
        else if (PlatformDetector.IsWindows)
        {
            Console.WriteLine();
            Console.WriteLine("📌 Consejos adicionales para Windows:");
            Console.WriteLine("  - Algunos comandos requieren ejecutar como Administrador");
            Console.WriteLine("  - Revisa el Visor de eventos: eventvwr.msc");
            Console.WriteLine("  - Monitorea con: Administrador de tareas (Ctrl+Shift+Esc)");
        }

        await Task.CompletedTask;
    }

    static async Task ExecuteSolution(ScanIssue issue, IDiagnosticEngine engine)
    {
        Console.WriteLine();
        Console.WriteLine("⚡ Ejecutando solución...");
        Console.WriteLine();

        // Extract and execute commands from solution
        var commands = ExtractCommands(issue.Solution);

        if (commands.Count == 0)
        {
            Console.WriteLine("  Esta solución no tiene comandos automáticos.");
            Console.WriteLine("  Sigue las instrucciones manuales proporcionadas.");
            await Task.CompletedTask;
            return;
        }

        foreach (var (fullCommand, needsSudo, isDiagnostic) in commands)
        {
            if (needsSudo && PlatformDetector.IsLinux)
            {
                Console.WriteLine($"  ⚠️  Este comando requiere permisos de root (sudo).");
                Console.WriteLine($"  Ejecuta manualmente:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    $ {fullCommand}");
                Console.ResetColor();
                Console.WriteLine();
                continue;
            }

            try
            {
                Console.WriteLine($"  📋 Ejecutando: {fullCommand}");
                var (output, exitCode) = await RunShellCommand(fullCommand);

                if (exitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✅ Comando ejecutado correctamente.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠️  Comando completado con código de salida: " + exitCode);
                    Console.ResetColor();
                }

                // Show full output for diagnostic commands
                if (!string.IsNullOrWhiteSpace(output))
                {
                    var outputLines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (isDiagnostic || outputLines.Length <= 30)
                    {
                        Console.WriteLine();
                        Console.WriteLine("  ┌─ SALIDA DEL COMANDO ─────────────────────────────┐");
                        foreach (var line in outputLines)
                        {
                            Console.WriteLine($"  │ {line}");
                        }
                        Console.WriteLine("  └──────────────────────────────────────────────────┘");
                    }
                    else
                    {
                        Console.WriteLine($"  (Salida: {outputLines.Length} líneas, mostrando las primeras 20)");
                        Console.WriteLine();
                        Console.WriteLine("  ┌─ SALIDA DEL COMANDO ─────────────────────────────┐");
                        foreach (var line in outputLines.Take(20))
                        {
                            Console.WriteLine($"  │ {line}");
                        }
                        Console.WriteLine("  └──────────────────────────────────────────────────┘");
                    }
                    Console.WriteLine();
                }

                // Special handling for high CPU: offer to kill top processes
                if (isDiagnostic && fullCommand.Contains("ps aux --sort=-%cpu"))
                {
                    await OfferKillTopProcesses(output);
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ❌ Error al ejecutar: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine($"  Ejecuta manualmente:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    $ {fullCommand}");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        await Task.CompletedTask;
    }

    /// Run a command through /bin/sh to support pipes, redirects, etc.
    static async Task<(string Output, int ExitCode)> RunShellCommand(string command, int timeoutSeconds = 15)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"{EscapeShell(command)}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null)
            return ("", -1);

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

        var completed = await Task.WhenAny(outputTask, timeoutTask);
        if (completed == timeoutTask)
        {
            try { process.Kill(); } catch { }
            return ("(Tiempo de espera agotado)", -1);
        }

        await process.WaitForExitAsync();
        var output = outputTask.Result;
        var error = await errorTask;

        var combined = output;
        if (!string.IsNullOrWhiteSpace(error) && error.Length < 500)
            combined += error;

        return (combined.Trim(), process.ExitCode);
    }

    static string EscapeShell(string cmd)
    {
        // Escape double quotes inside the command
        return cmd.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// Parse the ps aux output and offer to kill top CPU-consuming processes
    static async Task OfferKillTopProcesses(string psOutput)
    {
        var processes = ParsePsAux(psOutput);
        if (processes.Count == 0) return;

        Console.WriteLine();
        Console.WriteLine("  📊 Procesos que más CPU consumen:");
        Console.WriteLine();

        for (var i = 0; i < processes.Count; i++)
        {
            var p = processes[i];
            var color = p.CpuPercent >= 50 ? ConsoleColor.Red : p.CpuPercent >= 20 ? ConsoleColor.Yellow : ConsoleColor.Gray;
            Console.ForegroundColor = color;
            Console.WriteLine($"    [{i + 1}] PID {p.Pid,-8} CPU {p.CpuPercent,5}%  MEM {p.MemPercent,5}%  {p.ShortName}");
            Console.ResetColor();
        }

        // Show contextual suggestions based on detected processes
        ShowProcessSuggestions(processes);

        Console.WriteLine();
        Console.Write("  ¿Quieres terminar algún proceso? (número, 't' para todos >50%, o Enter para saltar): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) || input.ToLower() == "n") return;

        if (input.ToLower() == "t")
        {
            // Kill all processes using >50% CPU
            var toKill = processes.Where(p => p.CpuPercent > 50).ToList();
            if (toKill.Count == 0)
            {
                Console.WriteLine("  No hay procesos con más del 50% de CPU.");
                return;
            }
            Console.WriteLine();
            Console.Write($"  ⚠️  Esto terminará {toKill.Count} proceso(s). ¿Estás seguro? (s/n): ");
            var confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm == "s" || confirm == "si" || confirm == "y")
            {
                foreach (var p in toKill)
                {
                    await KillProcess(p.Pid);
                }
            }
        }
        else if (int.TryParse(input, out var choice) && choice > 0 && choice <= processes.Count)
        {
            var p = processes[choice - 1];
            Console.WriteLine();
            Console.Write($"  ⚠️  ¿Terminar PID {p.Pid} ({p.ShortName})? (s/n): ");
            var confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm == "s" || confirm == "si" || confirm == "y")
            {
                await KillProcess(p.Pid);
            }
        }
    }

    static async Task KillProcess(int pid)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "kill",
                Arguments = $"-15 {pid}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process != null)
            {
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✅ Proceso PID {pid} terminado correctamente.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ⚠️  No se pudo terminar PID {pid}: {error.Trim()}");
                    Console.ResetColor();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ❌ Error al terminar PID {pid}: {ex.Message}");
            Console.ResetColor();
        }
    }

    static List<(int Pid, double CpuPercent, double MemPercent, string Command, string ShortName)> ParsePsAux(string output)
    {
        var processes = new List<(int, double, double, string, string)>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Skip header line
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 11) continue;

            if (int.TryParse(parts[1], out var pid) &&
                double.TryParse(parts[2], out var cpu) &&
                double.TryParse(parts[3], out var mem))
            {
                var fullCommand = string.Join(" ", parts.Skip(10));
                var shortName = ShortenProcessName(fullCommand);
                processes.Add((pid, cpu, mem, fullCommand, shortName));
            }
        }

        return processes;
    }

    /// Shorten a process command line to a readable name
    static string ShortenProcessName(string fullCommand)
    {
        if (string.IsNullOrEmpty(fullCommand)) return "(desconocido)";

        // Extract the executable path
        var firstPart = fullCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? fullCommand;
        var exeName = System.IO.Path.GetFileName(firstPart);

        // Known process names with friendly labels
        var friendlyNames = new Dictionary<string, string>
        {
            { "chrome", "Google Chrome" },
            { "firefox", "Firefox" },
            { "junie", "Junie" },
            { "junie-app", "Junie" },
            { "Xorg", "X Server (Gráficos)" },
            { "xfce4-terminal", "Terminal XFCE" },
            { "dotnet", ".NET Runtime" },
            { "VBCSCompiler.dll", "C# Compilador" },
            { "code", "VS Code" },
            { "java", "Java" },
            { "node", "Node.js" },
            { "python3", "Python" },
            { "bash", "Bash" },
            { "sh", "Shell" },
            { "systemd", "Systemd" },
            { "sshd", "SSH Server" },
            { "docker", "Docker" },
            { "containerd", "Containerd" }
        };

        var label = friendlyNames.TryGetValue(exeName, out var v) ? v : exeName;

        // For Chrome/Chrome renderer processes, add context
        if (fullCommand.Contains("chrome --type=renderer"))
            label = "Chrome (Pestaña)";
        else if (fullCommand.Contains("chrome --type=gpu-process"))
            label = "Chrome (GPU)";
        else if (fullCommand.Contains("chrome --type=utility"))
            label = "Chrome (Utilidad)";

        return label;
    }

    /// Show contextual suggestions based on detected high-CPU processes
    static void ShowProcessSuggestions(List<(int Pid, double CpuPercent, double MemPercent, string Command, string ShortName)> processes)
    {
        var suggestions = new List<string>();

        foreach (var p in processes)
        {
            var cmd = p.Command.ToLower();

            // Chrome consuming a lot
            if (cmd.Contains("chrome") && p.CpuPercent > 30)
            {
                if (cmd.Contains("--type=renderer"))
                    suggestions.Add($"  🌐 {p.ShortName} usa {p.CpuPercent}% CPU — Cierra pestañas innecesarias o extensiones pesadas");
                else if (cmd.Contains("--type=gpu-process"))
                    suggestions.Add($"  🎮 {p.ShortName} usa {p.CpuPercent}% CPU — Desactiva aceleración por hardware en Chrome: chrome://settings/system");
            }

            // Junie consuming a lot
            if (cmd.Contains("junie") && p.CpuPercent > 20)
            {
                suggestions.Add($"  🤖 {p.ShortName} usa {p.CpuPercent}% CPU — Junie está procesando una tarea. Espera a que termine o reinicia la sesión");
            }

            // .NET compiler consuming a lot
            if (cmd.Contains("VBCSCompiler") && p.CpuPercent > 20)
            {
                suggestions.Add($"  🔨 {p.ShortName} usa {p.CpuPercent}% CPU — El compilador C# está trabajando. Espera a que termine la compilación");
            }

            // Docker consuming a lot
            if ((cmd.Contains("docker") || cmd.Contains("containerd")) && p.CpuPercent > 20)
            {
                suggestions.Add($"  🐳 {p.ShortName} usa {p.CpuPercent}% CPU — Un contenedor Docker está activo. Revisa con: docker ps");
            }

            // Xorg consuming a lot
            if (cmd.Contains("xorg") && p.CpuPercent > 20)
            {
                suggestions.Add($"  🖥️  {p.ShortName} usa {p.CpuPercent}% CPU — El servidor gráfico está sobrecargado. Cierra ventanas pesadas o reinicia la sesión");
            }
        }

        if (suggestions.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("  💡 Sugerencias:");
            foreach (var s in suggestions)
            {
                Console.WriteLine(s);
            }
        }
    }

    static List<(string FullCommand, bool NeedsSudo, bool IsDiagnostic)> ExtractCommands(string solution)
    {
        var commands = new List<(string, bool, bool)>();

        // Commands that are diagnostic (safe to run, show output)
        var diagnosticCommands = new HashSet<string>
        {
            "ps", "top", "free", "df", "du", "ls", "cat", "uname",
            "ip", "ifconfig", "netstat", "ss", "journalctl", "dmesg",
            "sensors", "smartctl", "lsblk", "lscpu", "uptime",
            "systemctl", "find", "grep"
        };

        // Commands that need sudo
        var sudoCommands = new HashSet<string>
        {
            "apt", "apt-get", "systemctl", "rm", "rmmod", "modprobe",
            "reboot", "shutdown", "fsck", "e2fsck", "fstrim",
            "journalctl", "smartctl"
        };

        foreach (var line in solution.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim().Trim('$').Trim();

            // Extract command after colon (e.g., "Identifica procesos: ps aux | head -10")
            var extractFrom = trimmed;
            if (trimmed.Contains(':'))
            {
                var afterColon = trimmed.Split(':', StringSplitOptions.RemoveEmptyEntries).Last().Trim();
                // Check if the part after colon looks like a command
                var firstWord = afterColon.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                if (IsKnownCommand(firstWord))
                {
                    extractFrom = afterColon;
                }
            }

            // Also check the full line for known commands at the start
            if (!IsKnownCommand(extractFrom.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? ""))
            {
                var firstWord = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                if (IsKnownCommand(firstWord))
                {
                    extractFrom = trimmed;
                }
                else
                {
                    continue;
                }
            }

            // Check if command needs sudo
            var firstCmd = extractFrom.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            var needsSudo = firstCmd == "sudo" || sudoCommands.Contains(firstCmd);
            var isDiagnostic = diagnosticCommands.Contains(firstCmd);

            commands.Add((extractFrom, needsSudo, isDiagnostic));
        }

        return commands;
    }

    static bool IsKnownCommand(string cmd)
    {
        var known = new HashSet<string>
        {
            "apt", "apt-get", "aptitude", "yum", "dnf", "pacman",
            "rm", "rmmod", "modprobe", "systemctl", "journalctl",
            "ps", "kill", "top", "htop", "free", "df", "du", "ls",
            "cat", "uname", "reboot", "shutdown", "ip", "ifconfig",
            "netstat", "ss", "smartctl", "fsck", "e2fsck", "fstrim",
            "sensors", "lsblk", "lscpu", "uptime", "find", "grep",
            "dmesg", "sudo"
        };
        return known.Contains(cmd);
    }

    /// Read the 1-minute load average from /proc/loadavg
    static async Task<double> GetLoadAverage()
    {
        try
        {
            var content = await File.ReadAllTextAsync("/proc/loadavg");
            var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && double.TryParse(parts[0], out var load))
                return load;
        }
        catch { }
        return -1;
    }
}
