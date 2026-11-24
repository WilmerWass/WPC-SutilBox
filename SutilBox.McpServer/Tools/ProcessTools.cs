using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SutilBox.McpServer.Tools;

[McpServerToolType]
public static class ProcessTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly HashSet<string> ProtectedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "System", "Idle", "Registry", "smss", "csrss", "wininit", "winlogon", "services",
        "lsass", "svchost", "explorer", "dwm", "fontdrvhost", "secure system", "sihost",
        "taskhostw", "RuntimeBroker", "audiodg", "spoolsv", "WmiPrvSE"
    };

    [McpServerTool(Name = "list_processes", Title = "List Running Processes")]
    [Description("Lista los procesos en ejecución ordenados por consumo de memoria RAM (Working Set), con opción de limitar la cantidad de resultados y filtrar por nombre.")]
    public static string ListProcesses(
        [Description("Número máximo de procesos a devolver (por defecto 20).")] int limit = 20,
        [Description("Filtro opcional por nombre del proceso.")] string? nameFilter = null)
    {
        try
        {
            var processes = Process.GetProcesses();
            var list = new List<object>();

            var filtered = processes
                .Where(p => string.IsNullOrWhiteSpace(nameFilter) || p.ProcessName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p =>
                {
                    try { return p.WorkingSet64; } catch { return 0L; }
                })
                .Take(Math.Max(1, Math.Min(limit, 100)));

            foreach (var p in filtered)
            {
                try
                {
                    list.Add(new
                    {
                        PID = p.Id,
                        Nombre = p.ProcessName,
                        Titulo_Ventana = string.IsNullOrWhiteSpace(p.MainWindowTitle) ? null : p.MainWindowTitle,
                        Memoria_MB = $"{p.WorkingSet64 / (1024.0 * 1024.0):0.00} MB",
                        Hilos = p.Threads.Count,
                        Prioridad = SafeGetPriority(p)
                    });
                }
                catch { }
            }

            return JsonSerializer.Serialize(list, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }

    [McpServerTool(Name = "kill_process", Title = "Kill Process by PID")]
    [Description("Termina un proceso no protegido tras confirmación explícita. Debe pasar confirm=true; la herramienta no puede solicitar confirmación interactiva.")]
    public static string KillProcess([Description("El identificador numérico de proceso (PID).")] int pid,
        [Description("Confirmación explícita del usuario para terminar este proceso.")] bool confirm = false)
    {
        if (!confirm)
            return JsonSerializer.Serialize(new { Exito = false, Mensaje = "Operación cancelada: establece confirm=true solo después de revisar el PID y aceptar terminar el proceso." }, JsonOptions);
        if (pid <= 4 || pid == Environment.ProcessId)
            return JsonSerializer.Serialize(new { Exito = false, Mensaje = "PID protegido o no válido." }, JsonOptions);
        try
        {
            var p = Process.GetProcessById(pid);
            string name = p.ProcessName;
            if (ProtectedProcesses.Contains(name))
                return JsonSerializer.Serialize(new { Exito = false, Mensaje = $"El proceso '{name}' está protegido y no se puede terminar desde esta herramienta." }, JsonOptions);
            p.Kill(entireProcessTree: false);
            return JsonSerializer.Serialize(new { Exito = true, Mensaje = $"El proceso '{name}' (PID: {pid}) ha sido terminado correctamente." }, JsonOptions);
        }
        catch (ArgumentException)
        {
            return JsonSerializer.Serialize(new { Exito = false, Mensaje = $"No se encontró ningún proceso con PID {pid}." }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Exito = false, Mensaje = $"Error al terminar proceso: {ex.Message}" }, JsonOptions);
        }
    }

    private static string SafeGetPriority(Process p)
    {
        try
        {
            return p.PriorityClass.ToString();
        }
        catch
        {
            return "N/A";
        }
    }
}
