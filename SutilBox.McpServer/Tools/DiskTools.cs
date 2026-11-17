using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SutilBox.McpServer.Tools;

[McpServerToolType]
public static class DiskTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "get_disk_usage", Title = "Get Disk Usage")]
    [Description("Obtiene el estado de almacenamiento de todas las unidades montadas y listas (espacio total, libre, ocupado y sistema de archivos).")]
    public static string GetDiskUsage()
    {
        var drives = DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => new
            {
                Nombre = d.Name,
                Etiqueta = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "(Sin etiqueta)" : d.VolumeLabel,
                Tipo = d.DriveType.ToString(),
                Sistema_Archivos = d.DriveFormat,
                Espacio_Total_GB = $"{d.TotalSize / 1_000_000_000.0:0.00} GB",
                Espacio_Libre_GB = $"{d.AvailableFreeSpace / 1_000_000_000.0:0.00} GB",
                Espacio_Usado_GB = $"{(d.TotalSize - d.AvailableFreeSpace) / 1_000_000_000.0:0.00} GB",
                Porcentaje_Uso = $"{(double)(d.TotalSize - d.AvailableFreeSpace) / d.TotalSize * 100:0.0}%"
            });

        return JsonSerializer.Serialize(drives, JsonOptions);
    }

    [McpServerTool(Name = "get_temp_folder_size", Title = "Get Temp Folder Size")]
    [Description("Calcula el tamaño total y la cantidad de archivos temporales en la carpeta %TEMP% del usuario.")]
    public static string GetTempFolderSize()
    {
        try
        {
            string tempPath = Path.GetTempPath();
            long totalSize = 0;
            int fileCount = 0;
            int skippedCount = 0;

            var dir = new DirectoryInfo(tempPath);
            foreach (var file in dir.EnumerateFiles("*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true }))
            {
                try
                {
                    totalSize += file.Length;
                    fileCount++;
                }
                catch
                {
                    skippedCount++;
                }
            }

            var result = new
            {
                Ruta = tempPath,
                Archivos_Analizados = fileCount,
                Archivos_Inaccesibles = skippedCount,
                Tamaño_Total_MB = $"{totalSize / 1_000_000.0:0.00} MB",
                Tamaño_Total_GB = $"{totalSize / 1_000_000_000.0:0.00} GB"
            };

            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }

    [McpServerTool(Name = "get_windows_directory_size", Title = "Get Windows Folder Size")]
    [Description("Analiza el tamaño de carpetas críticas del sistema de Windows (SoftwareDistribution, Temp, Logs, Prefetch, Installer).")]
    public static string GetWindowsDirectorySize()
    {
        try
        {
            string winDir = Environment.GetEnvironmentVariable("WINDIR") ?? "C:\\Windows";
            var folders = new[] { "Logs", "Temp", "SoftwareDistribution", "Installer", "Prefetch" };

            var results = new List<object>();
            foreach (var folder in folders)
            {
                string path = Path.Combine(winDir, folder);
                if (!Directory.Exists(path))
                {
                    results.Add(new { Carpeta = folder, Ruta = path, Existe = false, Tamaño_MB = "0.00 MB" });
                    continue;
                }

                try
                {
                    var dir = new DirectoryInfo(path);
                    long size = dir.EnumerateFiles("*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true })
                                   .Sum(f => f.Length);

                    results.Add(new { Carpeta = folder, Ruta = path, Existe = true, Tamaño_MB = $"{size / 1_000_000.0:0.00} MB" });
                }
                catch
                {
                    results.Add(new { Carpeta = folder, Ruta = path, Existe = true, Tamaño_MB = "No accesible (requiere permisos elevados)" });
                }
            }

            return JsonSerializer.Serialize(results, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = ex.Message }, JsonOptions);
        }
    }
}
