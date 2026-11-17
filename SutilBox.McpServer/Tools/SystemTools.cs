using System.ComponentModel;
using System.Text.Json;
using System.Management;
using ModelContextProtocol.Server;

namespace SutilBox.McpServer.Tools;

[McpServerToolType]
public static class SystemTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "get_system_info", Title = "Get System Information")]
    [Description("Obtiene la información general del hardware y software: equipo, SO, CPU, RAM, GPU, disco de sistema, BIOS, red y tiempo activo.")]
    public static string GetSystemInfo()
    {
        var info = new
        {
            Equipo = Environment.MachineName,
            Sistema_Operativo = $"{Environment.OSVersion} ({(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")})",
            Procesador = GetCpuName(),
            Memoria_RAM = GetTotalMemory(),
            Graficos = GetGpuName(),
            Disco_Sistema = GetSystemDiskInfo(),
            BIOS = GetBiosVersion(),
            Red = GetNetworkInfo(),
            Tiempo_Activo = GetUptime()
        };
        return JsonSerializer.Serialize(info, JsonOptions);
    }

    [McpServerTool(Name = "get_battery_info", Title = "Get Battery Information")]
    [Description("Obtiene el estado de la batería, porcentaje de carga, estado de conexión (AC/batería) y estimación de salud/capacidad.")]
    public static string GetBatteryInfo()
    {
        try
        {
            var batteryList = new List<object>();
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var query = searcher.Get();

            if (query.Count == 0)
            {
                return JsonSerializer.Serialize(new { Tiene_Bateria = false, Mensaje = "No se detectó batería en el equipo (equipo de escritorio o sin soporte WMI de batería)." }, JsonOptions);
            }

            foreach (var obj in query)
            {
                batteryList.Add(new
                {
                    Tiene_Bateria = true,
                    Nombre = obj["Name"]?.ToString() ?? "Batería del sistema",
                    Porcentaje_Carga = $"{obj["EstimatedChargeRemaining"]}%",
                    Estado_Bateria = GetBatteryStatusString(Convert.ToUInt16(obj["BatteryStatus"] ?? 0)),
                    Tiempo_Estimado_Min = obj["EstimatedRunTime"]?.ToString() ?? "N/A"
                });
            }

            return JsonSerializer.Serialize(batteryList, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }

    [McpServerTool(Name = "get_environment_variables", Title = "Get Environment Variables")]
    [Description("Obtiene las variables de entorno del sistema y usuario. Permite filtrar opcionalmente por prefijo.")]
    public static string GetEnvironmentVariables([Description("Prefijo opcional para filtrar las variables (ej: 'PATH', 'USER', 'DOTNET').")] string? prefix = null)
    {
        var envVars = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(kv => prefix == null || kv.Key.ToString()?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(kv => kv.Key.ToString())
            .Select(kv => new
            {
                Nombre = kv.Key,
                Valor = kv.Value
            });

        return JsonSerializer.Serialize(envVars, JsonOptions);
    }

    // Helper functions
    private static string GetCpuName()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            foreach (var item in searcher.Get())
            {
                return item["Name"]?.ToString() ?? "Desconocido";
            }
        }
        catch { }
        return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Desconocido";
    }

    private static string GetTotalMemory()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            long totalCapacity = 0;
            foreach (var item in searcher.Get())
            {
                if (long.TryParse(item["Capacity"]?.ToString(), out long cap))
                {
                    totalCapacity += cap;
                }
            }
            if (totalCapacity > 0) return FormatBytes(totalCapacity);
        }
        catch { }
        return "Desconocido";
    }

    private static string GetGpuName()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            foreach (var item in searcher.Get())
            {
                return item["Name"]?.ToString() ?? "Desconocido";
            }
        }
        catch { }
        return "Desconocido";
    }

    private static string GetSystemDiskInfo()
    {
        try
        {
            string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            var di = new DriveInfo(systemDrive);
            return $"{di.Name} {FormatBytes(di.AvailableFreeSpace)} libres de {FormatBytes(di.TotalSize)}";
        }
        catch { }
        return "Desconocido";
    }

    private static string GetBiosVersion()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion, Manufacturer FROM Win32_BIOS");
            foreach (var item in searcher.Get())
            {
                string ver = item["SMBIOSBIOSVersion"]?.ToString() ?? "";
                string man = item["Manufacturer"]?.ToString() ?? "";
                return $"{man} {ver}".Trim();
            }
        }
        catch { }
        return "N/A";
    }

    private static string GetNetworkInfo()
    {
        try
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) return "Desconectado";

            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && i.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                .OrderByDescending(i => i.GetIPProperties().GatewayAddresses.Count > 0)
                .FirstOrDefault();

            if (interfaces != null)
            {
                var props = interfaces.GetIPProperties();
                var ip = props.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return $"{interfaces.Name} ({ip?.Address})";
            }
        }
        catch { }
        return "Desconocido";
    }

    private static string GetUptime()
    {
        try
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }
        catch { }
        return "N/A";
    }

    private static string GetBatteryStatusString(ushort statusCode)
    {
        return statusCode switch
        {
            1 => "Descargando",
            2 => "Conectado a CA",
            3 => "Completamente cargada",
            4 => "Baja",
            5 => "Crítica",
            6 => "Cargando",
            7 => "Cargando y alta",
            8 => "Cargando y baja",
            9 => "Cargando y crítica",
            10 => "Indefinido",
            11 => "Parcialmente cargada",
            _ => "Desconocido"
        };
    }

    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
