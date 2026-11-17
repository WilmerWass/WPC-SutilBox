using System.ComponentModel;
using System.ServiceProcess;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SutilBox.McpServer.Tools;

[McpServerToolType]
public static class ServiceTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "list_services", Title = "List Windows Services")]
    [Description("Lista los servicios de Windows con su estado actual (Running, Stopped, etc.), tipo de servicio y capacidad de ser detenido.")]
    public static string ListServices(
        [Description("Filtro opcional por nombre o nombre visible del servicio.")] string? filter = null,
        [Description("Si es true, sólo lista los servicios que están ejecutándose actualmente (Running).")] bool runningOnly = false,
        [Description("Número máximo de servicios a devolver (por defecto 30).")] int limit = 30)
    {
        try
        {
            var services = ServiceController.GetServices();
            var list = services
                .Where(s => !runningOnly || s.Status == ServiceControllerStatus.Running)
                .Where(s => string.IsNullOrWhiteSpace(filter) ||
                            s.ServiceName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                            s.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.DisplayName)
                .Take(Math.Max(1, Math.Min(limit, 200)))
                .Select(s => new
                {
                    Nombre = s.ServiceName,
                    Nombre_Visible = s.DisplayName,
                    Estado = s.Status.ToString(),
                    Puede_Detenerse = s.CanStop,
                    Puede_Pausarse = s.CanPauseAndContinue
                });

            return JsonSerializer.Serialize(list, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }
}
