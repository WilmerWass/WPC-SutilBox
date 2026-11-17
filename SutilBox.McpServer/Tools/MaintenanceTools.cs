using System.ComponentModel;
using System.Text.Json;
using Microsoft.Win32;
using ModelContextProtocol.Server;

namespace SutilBox.McpServer.Tools;

[McpServerToolType]
public static class MaintenanceTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "list_startup_programs", Title = "List Startup Programs")]
    [Description("Lista los programas configurados para iniciar automáticamente con Windows desde el Registro (HKCU y HKLM) y carpetas de Inicio.")]
    public static string ListStartupPrograms()
    {
        try
        {
            var items = new List<object>();

            // HKCU Run
            ReadRegistryStartup(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "Usuario Actual (Run)", items);
            // HKLM Run
            ReadRegistryStartup(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "Toda la máquina (Run)", items);

            // Startup folder
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(startupFolder))
            {
                foreach (var file in Directory.GetFiles(startupFolder))
                {
                    items.Add(new
                    {
                        Nombre = Path.GetFileNameWithoutExtension(file),
                        Comando = file,
                        Ubicacion = "Carpeta de Inicio Usuario",
                        Tipo = "Acceso Directo / Archivo"
                    });
                }
            }

            return JsonSerializer.Serialize(items, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }

    [McpServerTool(Name = "get_privacy_status", Title = "Get Windows Privacy and Telemetry Settings")]
    [Description("Lee la configuración actual de privacidad, telemetría y diagnóstico en Windows.")]
    public static string GetPrivacyStatus()
    {
        try
        {
            var results = new List<object>();

            // Telemetría
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
            {
                var val = key?.GetValue("AllowTelemetry");
                results.Add(new
                {
                    Configuracion = "Telemetría de Windows (AllowTelemetry)",
                    Estado = val == null ? "Por defecto de Windows (No configurada por directiva)" : $"Valor Directiva: {val}"
                });
            }

            // Publicidad ID
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
            {
                var val = key?.GetValue("Enabled");
                results.Add(new
                {
                    Configuracion = "ID de Anuncios Personalizados",
                    Estado = val is int i && i == 1 ? "Activado" : (val is int ? "Desactivado" : "Por defecto")
                });
            }

            // Historial de actividad
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System"))
            {
                var val = key?.GetValue("EnableActivityFeed");
                results.Add(new
                {
                    Configuracion = "Historial de Actividades (ActivityFeed)",
                    Estado = val is int i && i == 0 ? "Desactivado por Directiva" : "Activo / Por defecto"
                });
            }

            return JsonSerializer.Serialize(results, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }

    private static void ReadRegistryStartup(RegistryKey root, string subKey, string location, List<object> items)
    {
        try
        {
            using var key = root.OpenSubKey(subKey);
            if (key == null) return;

            foreach (var valueName in key.GetValueNames())
            {
                var val = key.GetValue(valueName)?.ToString() ?? "";
                items.Add(new
                {
                    Nombre = valueName,
                    Comando = val,
                    Ubicacion = location,
                    Tipo = "Registro"
                });
            }
        }
        catch { }
    }
}
