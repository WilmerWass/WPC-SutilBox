using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public interface IBackupService
    {
        Task<BackupResult> CreateFullBackupAsync(string destinationPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<BackupResult> ExportDriversAsync(string destinationPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<BackupResult> ExportWifiProfilesAsync(string destinationPath, CancellationToken ct = default);
        Task<BackupResult> ExportSoftwareManifestAsync(string destinationPath, CancellationToken ct = default);
        Task<BackupResult> ExportUserSettingsAsync(string destinationPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<List<BackupInfo>> GetExistingBackupsAsync();
        Task<bool> DeleteBackupAsync(string backupPath);
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? BackupPath { get; set; }
        public long TotalBytesBackedUp { get; set; }
        public int ItemsCount { get; set; }
        public Dictionary<string, string> Details { get; set; } = new();
    }

    public class BackupInfo
    {
        public string Path { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public long TotalSize { get; set; }
        public int ItemsCount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class BackupManifest
    {
        public string Version { get; set; } = "1.0";
        public DateTime CreatedAt { get; set; }
        public string SourceMachine { get; set; } = Environment.MachineName;
        public string SourceUser { get; set; } = Environment.UserName;
        public string WindowsVersion { get; set; } = string.Empty;
        public List<SoftwareEntry> InstalledSoftware { get; set; } = new();
        public List<DriverEntry> Drivers { get; set; } = new();
        public List<string> WifiProfiles { get; set; } = new();
        public List<string> BackedUpFolders { get; set; } = new();
        public long TotalSizeBytes { get; set; }
    }

    public class SoftwareEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string? InstallLocation { get; set; }
        public string? UninstallString { get; set; }
        public string? WingetId { get; set; }
    }

    public class DriverEntry
    {
        public string Name { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Version { get; set; }
    }

    public class BackupService : IBackupService
    {
        private readonly ILogService _log;
        private readonly IDriverService _driverService;

        public BackupService(ILogService log, IDriverService driverService)
        {
            _log = log;
            _driverService = driverService;
        }

        public async Task<BackupResult> CreateFullBackupAsync(string destinationPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new BackupResult { BackupPath = destinationPath };

            try
            {
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                _log.Info($"[Backup] Iniciando copia de seguridad completa en: {destinationPath}");
                progress?.Report((0, "Iniciando copia de seguridad completa..."));

                // 1. Exportar drivers (pesado - 40%)
                progress?.Report((10, "Paso 1/4: Exportando controladores..."));
                var driverProgress = new Progress<(int Percentage, string Message)>(p =>
                    progress?.Report((10 + (p.Percentage * 30 / 100), $"Exportando controladores... {p.Message}")));
                var driverResult = await ExportDriversAsync(Path.Combine(destinationPath, "Drivers"), driverProgress, ct);
                if (driverResult.Success)
                {
                    result.Details["Drivers"] = "Exportados correctamente";
                    result.TotalBytesBackedUp += driverResult.TotalBytesBackedUp;
                }
                else
                {
                    result.Details["Drivers"] = $"Error: {driverResult.Message}";
                }

                // 2. Exportar perfiles Wi-Fi (5%)
                progress?.Report((40, "Paso 2/4: Exportando configuraciones Wi-Fi..."));
                var wifiResult = await ExportWifiProfilesAsync(Path.Combine(destinationPath, "WifiProfiles"), ct);
                if (wifiResult.Success)
                {
                    result.Details["Wifi"] = $"{wifiResult.ItemsCount} perfiles exportados";
                    result.ItemsCount += wifiResult.ItemsCount;
                }
                else
                {
                    result.Details["Wifi"] = $"Error: {wifiResult.Message}";
                }
                progress?.Report((50, "Perfiles Wi-Fi exportados."));

                // 3. Exportar manifiesto de software (5%)
                progress?.Report((55, "Paso 3/4: Creando manifiesto de software..."));
                var manifestResult = await ExportSoftwareManifestAsync(Path.Combine(destinationPath, "Software"), ct);
                if (manifestResult.Success)
                {
                    result.Details["Software"] = $"{manifestResult.ItemsCount} programas detectados";
                    result.ItemsCount += manifestResult.ItemsCount;
                }
                else
                {
                    result.Details["Software"] = $"Error: {manifestResult.Message}";
                }
                progress?.Report((70, "Manifiesto de software creado."));

                // 4. Exportar configuraciones del usuario (30%)
                progress?.Report((75, "Paso 4/4: Respaldo de configuraciones del usuario..."));
                var settingsProgress = new Progress<(int Percentage, string Message)>(p =>
                    progress?.Report((75 + (p.Percentage * 25 / 100), $"Respaldo de configuraciones... {p.Message}")));
                var settingsResult = await ExportUserSettingsAsync(Path.Combine(destinationPath, "UserSettings"), settingsProgress, ct);
                if (settingsResult.Success)
                {
                    result.Details["UserSettings"] = $"{settingsResult.ItemsCount} configuraciones respaldadas";
                    result.ItemsCount += settingsResult.ItemsCount;
                    result.TotalBytesBackedUp += settingsResult.TotalBytesBackedUp;
                }
                else
                {
                    result.Details["UserSettings"] = $"Error: {settingsResult.Message}";
                }

                // Generar manifiesto JSON
                progress?.Report((98, "Generando resumen del backup..."));
                await GenerateManifest(destinationPath, result);

                result.Success = true;
                result.Message = $"Copia de seguridad completada en: {destinationPath}";
                progress?.Report((100, "Copia de seguridad completada con éxito."));
                _log.Info($"[Backup] Backup completado. Items: {result.ItemsCount}, Bytes: {result.TotalBytesBackedUp}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Copia de seguridad cancelada por el usuario.";
                _log.Info("[Backup] Backup cancelado.");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error crítico: {ex.Message}";
                _log.Error("[Backup] Error en copia de seguridad completa", ex);
            }

            return result;
        }

        public async Task<BackupResult> ExportDriversAsync(string destinationPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new BackupResult();
            var driverProgress = new Progress<(int, string)>(p => progress?.Report((p.Item1, p.Item2)));
            var driverResult = await _driverService.ExportDriversAsync(destinationPath, driverProgress, ct);
            result.Success = driverResult.Success;
            result.Message = driverResult.Message;

            if (result.Success)
            {
                result.TotalBytesBackedUp = GetDirectorySize(destinationPath);
                result.ItemsCount = CountFiles(destinationPath);
            }

            return result;
        }

        public async Task<BackupResult> ExportWifiProfilesAsync(string destinationPath, CancellationToken ct = default)
        {
            var result = new BackupResult();
            try
            {
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                // Listar perfiles Wi-Fi
                var listPsi = new ProcessStartInfo("netsh", "wlan show profiles")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var profiles = new List<string>();
                using (var process = Process.Start(listPsi))
                {
                    if (process == null) throw new InvalidOperationException("No se pudo ejecutar netsh");
                    ct.ThrowIfCancellationRequested();
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync(ct);

                    // Parsear nombres de perfiles
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var match = Regex.Match(line, @"All User Profile\s*:\s*(.+)");
                        if (match.Success)
                            profiles.Add(match.Groups[1].Value.Trim());
                    }
                }

                // Exportar cada perfil
                result.ItemsCount = 0;
                foreach (var profile in profiles)
                {
                    ct.ThrowIfCancellationRequested();
                    var exportPsi = new ProcessStartInfo("netsh", $"wlan export profile name=\"{profile}\" folder=\"{destinationPath}\" key=clear")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(exportPsi))
                    {
                        if (process != null)
                        {
                            await process.WaitForExitAsync(ct);
                            result.ItemsCount++;
                        }
                    }
                }

                result.Success = true;
                result.Message = $"{result.ItemsCount} perfiles Wi-Fi exportados.";
                result.TotalBytesBackedUp = GetDirectorySize(destinationPath);
                _log.Info($"[Backup] Perfiles Wi-Fi exportados: {result.ItemsCount}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Exportación de Wi-Fi cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al exportar perfiles Wi-Fi: {ex.Message}";
                _log.Error("[Backup] Error exportando Wi-Fi", ex);
            }

            return result;
        }

        public async Task<BackupResult> ExportSoftwareManifestAsync(string destinationPath, CancellationToken ct = default)
        {
            var result = new BackupResult();
            try
            {
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                var softwareList = new List<SoftwareEntry>();

                // Obtener software desde registro (x64 y x86)
                var keys = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (var keyPath in keys)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        using var rootKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
                        if (rootKey == null) continue;

                        foreach (var subKeyName in rootKey.GetSubKeyNames())
                        {
                            try
                            {
                                using var subKey = rootKey.OpenSubKey(subKeyName);
                                if (subKey == null) continue;

                                var name = subKey.GetValue("DisplayName") as string;
                                if (string.IsNullOrWhiteSpace(name)) continue;

                                var entry = new SoftwareEntry
                                {
                                    Name = name,
                                    Version = subKey.GetValue("DisplayVersion") as string ?? "",
                                    Publisher = subKey.GetValue("Publisher") as string ?? "",
                                    InstallLocation = subKey.GetValue("InstallLocation") as string,
                                    UninstallString = subKey.GetValue("UninstallString") as string
                                };

                                softwareList.Add(entry);
                            }
                            catch { /* Saltar claves con permisos insuficientes */ }
                        }
                    }
                    catch { /* Ignorar errores de registro */ }
                }

                // Guardar manifiesto JSON
                var manifestPath = Path.Combine(destinationPath, "installed_software.json");
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(softwareList, options), ct);

                // Guardar lista simple de Winget IDs
                var wingetListPath = Path.Combine(destinationPath, "winget_ids.txt");
                var wingetLines = new List<string>();
                foreach (var sw in softwareList)
                {
                    if (!string.IsNullOrEmpty(sw.Name))
                        wingetLines.Add(sw.Name);
                }
                await File.WriteAllTextAsync(wingetListPath, string.Join(Environment.NewLine, wingetLines), ct);

                result.Success = true;
                result.ItemsCount = softwareList.Count;
                result.Message = $"{softwareList.Count} programas detectados y guardados.";
                result.TotalBytesBackedUp = GetDirectorySize(destinationPath);
                _log.Info($"[Backup] Manifiesto de software: {softwareList.Count} programas");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Exportación de manifiesto cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al exportar manifiesto: {ex.Message}";
                _log.Error("[Backup] Error exportando manifiesto de software", ex);
            }

            return result;
        }

        public async Task<BackupResult> ExportUserSettingsAsync(string destinationPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new BackupResult();
            try
            {
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                var foldersToBackup = new Dictionary<string, string>
                {
                    { "AppData_Roaming", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..") },
                    { "AppData_Local", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) },
                };

                var itemsBackedUp = 0;
                var totalFolders = foldersToBackup.Count;
                var folderIndex = 0;

                foreach (var (folderName, sourcePath) in foldersToBackup)
                {
                    ct.ThrowIfCancellationRequested();
                    var destFolder = Path.Combine(destinationPath, folderName);

                    // Solo respaldar carpetas específicas de configuración, no todo AppData
                    var appsToBackup = new[]
                    {
                        "Discord", "Steam", "EpicGames", "Microsoft", "Google", "Mozilla Firefox",
                        "Opera Software", "Spotify", "WhatsApp", "Telegram Desktop", "BraveSoftware",
                        "Adobe", "Valve", "NVIDIA", "AMD", "Intel", "Microsoft VS Code",
                        "JetBrains", "Microsoft Office", "Skype", "TeamViewer", "AnyDesk"
                    };

                    progress?.Report(((folderIndex * 100) / totalFolders, $"Copiando configuraciones de {folderName}..."));

                    foreach (var appName in appsToBackup)
                    {
                        ct.ThrowIfCancellationRequested();
                        var appSource = Path.Combine(sourcePath, appName);
                        if (Directory.Exists(appSource))
                        {
                            var appDest = Path.Combine(destFolder, appName);
                            await CopyDirectoryAsync(appSource, appDest, ct);
                            itemsBackedUp++;
                        }
                    }

                    folderIndex++;
                    progress?.Report((((folderIndex * 100) / totalFolders), $"{folderName} completado."));
                }

                // Exportar registros de Windows clave
                var registryPath = Path.Combine(destinationPath, "Registry");
                if (!Directory.Exists(registryPath))
                    Directory.CreateDirectory(registryPath);

                var registryKeys = new[]
                {
                    ("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer", "explorer_settings.reg"),
                    ("HKCU\\Control Panel\\Desktop", "desktop_settings.reg"),
                    ("HKCU\\Control Panel\\Colors", "colors_settings.reg"),
                };

                foreach (var (keyPath, fileName) in registryKeys)
                {
                    ct.ThrowIfCancellationRequested();
                    var regFile = Path.Combine(registryPath, fileName);
                    var psi = new ProcessStartInfo("reg", $"export \"{keyPath}\" \"{regFile}\" /y")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    using (var process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            await process.WaitForExitAsync(ct);
                            if (process.ExitCode == 0 && File.Exists(regFile))
                                itemsBackedUp++;
                        }
                    }
                }

                result.Success = true;
                result.ItemsCount = itemsBackedUp;
                result.TotalBytesBackedUp = GetDirectorySize(destinationPath);
                result.Message = $"{itemsBackedUp} configuraciones respaldadas.";
                _log.Info($"[Backup] Configuraciones de usuario: {itemsBackedUp} items");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Exportación de configuraciones cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al exportar configuraciones: {ex.Message}";
                _log.Error("[Backup] Error exportando configuraciones de usuario", ex);
            }

            return result;
        }

        public async Task<List<BackupInfo>> GetExistingBackupsAsync()
        {
            return await Task.Run(() =>
            {
                var backups = new List<BackupInfo>();
                var backupRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "WPC-SutilBox Backups");

                if (!Directory.Exists(backupRoot))
                    return backups;

                foreach (var dir in Directory.EnumerateDirectories(backupRoot, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var manifestPath = Path.Combine(dir, "backup_manifest.json");
                        var info = new BackupInfo
                        {
                            Path = dir,
                            CreatedDate = Directory.GetCreationTime(dir),
                            TotalSize = GetDirectorySize(dir),
                            Description = $"Backup de {new DirectoryInfo(dir).Name}"
                        };

                        if (File.Exists(manifestPath))
                        {
                            var json = File.ReadAllText(manifestPath);
                            var manifest = JsonSerializer.Deserialize<BackupManifest>(json);
                            if (manifest != null)
                            {
                                info.ItemsCount = manifest.InstalledSoftware.Count + manifest.WifiProfiles.Count;
                                info.Description = $"Backup creado el {manifest.CreatedAt:dd/MM/yyyy HH:mm}";
                            }
                        }

                        backups.Add(info);
                    }
                    catch { /* Saltar directorios inválidos */ }
                }

                return backups;
            });
        }

        public async Task<bool> DeleteBackupAsync(string backupPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(backupPath))
                        Directory.Delete(backupPath, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task GenerateManifest(string destinationPath, BackupResult result)
        {
            try
            {
                var manifest = new BackupManifest
                {
                    CreatedAt = DateTime.Now,
                    TotalSizeBytes = result.TotalBytesBackedUp
                };

                // Obtener versión de Windows
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        manifest.WindowsVersion = obj["Caption"]?.ToString() ?? "";
                        break;
                    }
                }
                catch { manifest.WindowsVersion = "Windows (desconocida)"; }

                // Cargar software si existe
                var softwarePath = Path.Combine(destinationPath, "Software", "installed_software.json");
                if (File.Exists(softwarePath))
                {
                    var json = await File.ReadAllTextAsync(softwarePath);
                    manifest.InstalledSoftware = JsonSerializer.Deserialize<List<SoftwareEntry>>(json) ?? new List<SoftwareEntry>();
                }

                // Contar perfiles Wi-Fi
                var wifiPath = Path.Combine(destinationPath, "WifiProfiles");
                if (Directory.Exists(wifiPath))
                {
                    manifest.WifiProfiles = Directory.GetFiles(wifiPath, "*.xml").Select(f => Path.GetFileName(f)!).ToList();
                }

                // Contar carpetas respaldadas
                var settingsPath = Path.Combine(destinationPath, "UserSettings");
                if (Directory.Exists(settingsPath))
                {
                    manifest.BackedUpFolders = Directory.EnumerateDirectories(settingsPath).Select(d => Path.GetFileName(d)!).ToList();
                }

                var manifestFilePath = Path.Combine(destinationPath, "backup_manifest.json");
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                await File.WriteAllTextAsync(manifestFilePath, JsonSerializer.Serialize(manifest, options));
            }
            catch (Exception ex)
            {
                _log.Error("[Backup] Error generando manifiesto", ex);
            }
        }

        private async Task CopyDirectoryAsync(string sourceDir, string destDir, CancellationToken ct)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (var file in Directory.EnumerateFiles(sourceDir))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var destFile = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
                catch { /* Saltar archivos con permisos insuficientes */ }
            }

            foreach (var subDir in Directory.EnumerateDirectories(sourceDir))
            {
                ct.ThrowIfCancellationRequested();
                var subDest = Path.Combine(destDir, new DirectoryInfo(subDir).Name);
                await CopyDirectoryAsync(subDir, subDest, ct);
            }
        }

        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long total = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { total += new FileInfo(file).Length; }
                    catch { }
                }
            }
            catch { }
            return total;
        }

        private int CountFiles(string path)
        {
            if (!Directory.Exists(path)) return 0;
            try
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
            }
            catch { return 0; }
        }
    }
}
