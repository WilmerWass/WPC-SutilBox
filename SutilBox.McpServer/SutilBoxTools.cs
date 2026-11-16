using System.ComponentModel;
using System.Text.Json;
using System.Management;
using ModelContextProtocol.Server;

namespace SutilBox.McpServer;

[McpServerToolType]
public static class SutilBoxTools
{
    [McpServerTool(Name = "get_system_info", Title = "Get System Information")]
    [Description("Obtiene la información general del sistema: nombre del equipo, SO, procesador, RAM, GPU, disco, BIOS, red y tiempo activo.")]
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
        return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "get_disk_usage", Title = "Get Disk Usage")]
    [Description("Obtiene el uso de disco de todas las unidades disponibles, incluyendo espacio total, usado y libre.")]
    public static string GetDiskUsage()
    {
        var drives = System.IO.DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => new
            {
                Nombre = d.Name,
                Tipo = d.DriveType.ToString(),
                Sistema_Archivos = d.DriveFormat,
                Espacio_Total_GB = $"{d.TotalSize / 1_000_000_000.0:0.00}",
                Espacio_Libre_GB = $"{d.AvailableFreeSpace / 1_000_000_000.0:0.00}",
                Espacio_Usado_GB = $"{(d.TotalSize - d.AvailableFreeSpace) / 1_000_000_000.0:0.00}"
            });

        return JsonSerializer.Serialize(drives, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "get_environment_variables", Title = "Get Environment Variables")]
    [Description("Obtiene las variables de entorno del sistema. Opcionalmente se puede filtrar por un prefijo.")]
    public static string GetEnvironmentVariables([Description("Prefijo opcional para filtrar las variables de entorno.")] string? prefix = null)
    {
        var envVars = System.Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(kv => prefix == null || kv.Key.ToString()?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true)
            .Select(kv => new
            {
                Nombre = kv.Key,
                Valor = kv.Value
            });

        return JsonSerializer.Serialize(envVars, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "get_temp_folder_size", Title = "Get Temp Folder Size")]
    [Description("Calcula el tamaño total de los archivos en la carpeta temporal del usuario y devuelve la cantidad de archivos.")]
    public static string GetTempFolderSize()
    {
        try
        {
            string tempPath = System.IO.Path.GetTempPath();
            long totalSize = 0;
            int fileCount = 0;

            try
            {
                var dir = new System.IO.DirectoryInfo(tempPath);
                foreach (var file in dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories))
                {
                    totalSize += file.Length;
                    fileCount++;
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                // Skip folders we can't access
            }

            var result = new
            {
                Ruta = tempPath,
                Archivos = fileCount,
                Tamaño_Total_GB = $"{totalSize / 1_000_000_000.0:0.00}",
                Tamaño_Total_MB = $"{totalSize / 1_000_000.0:0.00}"
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error al calcular el tamaño de la carpeta temporal: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_windows_directory_size", Title = "Get Windows Folder Size")]
    [Description("Calcula el tamaño de subcarpetas principales dentro de la carpeta de Windows.")]
    public static string GetWindowsDirectorySize()
    {
        try
        {
            string winDir = Environment.GetEnvironmentVariable("WINDIR") ?? "C:\\Windows";
            var folders = new[] { "Logs", "Temp", "SoftwareDistribution", "Installer", "Prefetch" };

            var results = new List<object>();
            foreach (var folder in folders)
            {
                string path = System.IO.Path.Combine(winDir, folder);
                if (!System.IO.Directory.Exists(path))
                {
                    results.Add(new { Carpeta = folder, Existe = false, Tamaño_MB = "0.00" });
                    continue;
                }

                try
                {
                    long size = new System.IO.DirectoryInfo(path)
                        .EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
                        .Sum(f => f.Length);
                    results.Add(new { Carpeta = folder, Existe = true, Tamaño_MB = $"{size / 1_000_000.0:0.00}" });
                }
                catch
                {
                    results.Add(new { Carpeta = folder, Existe = true, Tamaño_MB = "No accesible" });
                }
            }

            return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // --- Private helpers ---

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
            string systemDrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            var di = new System.IO.DriveInfo(systemDrive);
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

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
