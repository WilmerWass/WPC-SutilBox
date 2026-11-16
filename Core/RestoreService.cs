using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public interface IRestoreService
    {
        Task<RestoreResult> RestoreFromBackupAsync(string backupPath, RestoreOptions options, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<RestoreResult> ImportDriversAsync(string driversPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<RestoreResult> ImportWifiProfilesAsync(string wifiPath, CancellationToken ct = default);
        Task<RestoreResult> ReinstallSoftwareAsync(string softwarePath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<RestoreResult> RestoreUserSettingsAsync(string settingsPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default);
        Task<BackupManifest?> LoadBackupManifestAsync(string backupPath);
    }

    public class RestoreResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string> Details { get; set; } = new();
        public int ItemsRestored { get; set; }
    }

    public class RestoreOptions
    {
        public bool RestoreDrivers { get; set; } = true;
        public bool RestoreWifi { get; set; } = true;
        public bool ReinstallSoftware { get; set; } = true;
        public bool RestoreUserSettings { get; set; } = true;
    }

    public class RestoreService : IRestoreService
    {
        private readonly ILogService _log;
        private readonly IWingetService _wingetService;

        public RestoreService(ILogService log, IWingetService wingetService)
        {
            _log = log;
            _wingetService = wingetService;
        }

        public async Task<BackupManifest?> LoadBackupManifestAsync(string backupPath)
        {
            try
            {
                var manifestPath = Path.Combine(backupPath, "backup_manifest.json");
                if (!File.Exists(manifestPath))
                    return null;

                var json = await File.ReadAllTextAsync(manifestPath);
                return JsonSerializer.Deserialize<BackupManifest>(json);
            }
            catch (Exception ex)
            {
                _log.Error("[Restore] Error cargando manifiesto", ex);
                return null;
            }
        }

        public async Task<RestoreResult> RestoreFromBackupAsync(string backupPath, RestoreOptions options, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new RestoreResult();
            try
            {
                if (!Directory.Exists(backupPath))
                {
                    result.Success = false;
                    result.Message = "La carpeta de backup no existe.";
                    return result;
                }

                _log.Info($"[Restore] Iniciando restauración desde: {backupPath}");
                progress?.Report((0, "Iniciando restauración desde backup..."));

                var steps = new List<bool>();
                if (options.RestoreDrivers) steps.Add(true);
                if (options.RestoreWifi) steps.Add(true);
                if (options.ReinstallSoftware) steps.Add(true);
                if (options.RestoreUserSettings) steps.Add(true);

                var currentStep = 0;
                var totalSteps = steps.Count;
                if (totalSteps == 0)
                {
                    result.Success = true;
                    result.Message = "No se seleccionó nada para restaurar.";
                    return result;
                }

                // 1. Importar drivers
                if (options.RestoreDrivers)
                {
                    var driversPath = Path.Combine(backupPath, "Drivers");
                    if (Directory.Exists(driversPath))
                    {
                        progress?.Report(((currentStep * 100 / totalSteps), "Paso 1: Importando controladores..."));
                        var driverProgress = new Progress<(int Percentage, string Message)>(p =>
                            progress?.Report(((currentStep * 100 / totalSteps) + (p.Percentage * 100 / totalSteps / 100), $"Importando controladores... {p.Message}")));
                        var driverResult = await ImportDriversAsync(driversPath, driverProgress, ct);
                        if (driverResult.Success)
                        {
                            result.Details["Drivers"] = "Controladores importados correctamente";
                            result.ItemsRestored += driverResult.ItemsRestored;
                        }
                        else
                        {
                            result.Details["Drivers"] = $"Error: {driverResult.Message}";
                        }
                    }
                    else
                    {
                        result.Details["Drivers"] = "No se encontraron drivers en el backup";
                    }
                    currentStep++;
                }

                // 2. Importar perfiles Wi-Fi
                if (options.RestoreWifi)
                {
                    var wifiPath = Path.Combine(backupPath, "WifiProfiles");
                    if (Directory.Exists(wifiPath))
                    {
                        progress?.Report(((currentStep * 100 / totalSteps), "Paso 2: Importando configuraciones Wi-Fi..."));
                        var wifiResult = await ImportWifiProfilesAsync(wifiPath, ct);
                        if (wifiResult.Success)
                        {
                            result.Details["Wifi"] = $"{wifiResult.ItemsRestored} perfiles importados";
                            result.ItemsRestored += wifiResult.ItemsRestored;
                        }
                        else
                        {
                            result.Details["Wifi"] = $"Error: {wifiResult.Message}";
                        }
                    }
                    else
                    {
                        result.Details["Wifi"] = "No se encontraron perfiles Wi-Fi en el backup";
                    }
                    currentStep++;
                }

                // 3. Reinstalar software
                if (options.ReinstallSoftware)
                {
                    var softwarePath = Path.Combine(backupPath, "Software");
                    if (Directory.Exists(softwarePath))
                    {
                        progress?.Report(((currentStep * 100 / totalSteps), "Paso 3: Reinstalando programas..."));
                        var softwareProgress = new Progress<(int Percentage, string Message)>(p =>
                            progress?.Report(((currentStep * 100 / totalSteps) + (p.Percentage * 100 / totalSteps / 100), $"Reinstalando programas... {p.Message}")));
                        var softwareResult = await ReinstallSoftwareAsync(softwarePath, softwareProgress, ct);
                        if (softwareResult.Success)
                        {
                            result.Details["Software"] = $"{softwareResult.ItemsRestored} programas reinstalados";
                            result.ItemsRestored += softwareResult.ItemsRestored;
                        }
                        else
                        {
                            result.Details["Software"] = $"Error: {softwareResult.Message}";
                        }
                    }
                    else
                    {
                        result.Details["Software"] = "No se encontró manifiesto de software en el backup";
                    }
                    currentStep++;
                }

                // 4. Restaurar configuraciones de usuario
                if (options.RestoreUserSettings)
                {
                    var settingsPath = Path.Combine(backupPath, "UserSettings");
                    if (Directory.Exists(settingsPath))
                    {
                        progress?.Report(((currentStep * 100 / totalSteps), "Paso 4: Restaurando configuraciones de usuario..."));
                        var settingsProgress = new Progress<(int Percentage, string Message)>(p =>
                            progress?.Report(((currentStep * 100 / totalSteps) + (p.Percentage * 100 / totalSteps / 100), $"Restaurando configuraciones... {p.Message}")));
                        var settingsResult = await RestoreUserSettingsAsync(settingsPath, settingsProgress, ct);
                        if (settingsResult.Success)
                        {
                            result.Details["UserSettings"] = $"{settingsResult.ItemsRestored} configuraciones restauradas";
                            result.ItemsRestored += settingsResult.ItemsRestored;
                        }
                        else
                        {
                            result.Details["UserSettings"] = $"Error: {settingsResult.Message}";
                        }
                    }
                    else
                    {
                        result.Details["UserSettings"] = "No se encontraron configuraciones en el backup";
                    }
                    currentStep++;
                }

                result.Success = true;
                result.Message = $"Restauración completada. {result.ItemsRestored} items restaurados.";
                progress?.Report((100, "Restauración completada con éxito."));
                _log.Info($"[Restore] Restauración completada. Items: {result.ItemsRestored}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Restauración cancelada por el usuario.";
                _log.Info("[Restore] Restauración cancelada.");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error crítico: {ex.Message}";
                _log.Error("[Restore] Error en restauración", ex);
            }

            return result;
        }

        public async Task<RestoreResult> ImportDriversAsync(string driversPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new RestoreResult();
            try
            {
                if (!Directory.Exists(driversPath))
                {
                    result.Success = false;
                    result.Message = "La carpeta de drivers no existe.";
                    return result;
                }

                progress?.Report((0, "Importando controladores con PNPUTIL..."));

                // Usar pnputil para importar drivers
                var psi = new ProcessStartInfo("pnputil", $"/add-driver \"{driversPath}\" /subdirectories /install")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        result.Success = false;
                        result.Message = "No se pudo ejecutar pnputil.";
                        return result;
                    }

                    var output = await process.StandardOutput.ReadToEndAsync(ct);
                    await process.WaitForExitAsync(ct);

                    // Contar drivers importados
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Contains("Driver package added", StringComparison.OrdinalIgnoreCase) ||
                            line.Contains("Paquete de controlador agregado", StringComparison.OrdinalIgnoreCase))
                        {
                            result.ItemsRestored++;
                        }
                    }

                    if (process.ExitCode == 0 || result.ItemsRestored > 0)
                    {
                        result.Success = true;
                        result.Message = $"{result.ItemsRestored} controladores importados.";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = $"pnputil falló con código {process.ExitCode}.";
                    }
                }

                _log.Info($"[Restore] Drivers importados: {result.ItemsRestored}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Importación de drivers cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al importar drivers: {ex.Message}";
                _log.Error("[Restore] Error importando drivers", ex);
            }

            return result;
        }

        public async Task<RestoreResult> ImportWifiProfilesAsync(string wifiPath, CancellationToken ct = default)
        {
            var result = new RestoreResult();
            try
            {
                if (!Directory.Exists(wifiPath))
                {
                    result.Success = false;
                    result.Message = "La carpeta de perfiles Wi-Fi no existe.";
                    return result;
                }

                var xmlFiles = Directory.GetFiles(wifiPath, "*.xml");
                result.ItemsRestored = 0;

                foreach (var xmlFile in xmlFiles)
                {
                    ct.ThrowIfCancellationRequested();

                    var psi = new ProcessStartInfo("netsh", $"wlan add profile filename=\"{xmlFile}\"")
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
                            if (process.ExitCode == 0)
                                result.ItemsRestored++;
                        }
                    }
                }

                result.Success = true;
                result.Message = $"{result.ItemsRestored} perfiles Wi-Fi importados.";
                _log.Info($"[Restore] Perfiles Wi-Fi importados: {result.ItemsRestored}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Importación de Wi-Fi cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al importar perfiles Wi-Fi: {ex.Message}";
                _log.Error("[Restore] Error importando Wi-Fi", ex);
            }

            return result;
        }

        public async Task<RestoreResult> ReinstallSoftwareAsync(string softwarePath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new RestoreResult();
            try
            {
                var manifestPath = Path.Combine(softwarePath, "installed_software.json");
                if (!File.Exists(manifestPath))
                {
                    result.Success = false;
                    result.Message = "No se encontró el manifiesto de software.";
                    return result;
                }

                var json = await File.ReadAllTextAsync(manifestPath, ct);
                var softwareList = JsonSerializer.Deserialize<List<SoftwareEntry>>(json) ?? new List<SoftwareEntry>();

                // Filtrar solo los que tienen Winget ID
                var wingetIds = new List<string>();
                foreach (var sw in softwareList)
                {
                    if (!string.IsNullOrEmpty(sw.Name))
                        wingetIds.Add(sw.Name);
                }

                result.ItemsRestored = 0;
                var total = wingetIds.Count;
                var processed = 0;

                // Intentar instalar cada programa con winget
                foreach (var appName in wingetIds)
                {
                    ct.ThrowIfCancellationRequested();

                    progress?.Report(((processed * 100) / Math.Max(1, total), $"Instalando {appName}..."));

                    var psi = new ProcessStartInfo("winget", $"install --id \"{appName}\" --accept-source-agreements --accept-package-agreements --silent")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    };

                    using (var process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            await process.WaitForExitAsync(ct);
                            // Exit code 0 = success, 9 = already installed
                            if (process.ExitCode == 0 || process.ExitCode == 9)
                                result.ItemsRestored++;
                        }
                    }

                    processed++;
                }

                result.Success = true;
                result.Message = $"{result.ItemsRestored} de {total} programas reinstalados.";
                _log.Info($"[Restore] Software reinstalado: {result.ItemsRestored}/{total}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Reinstalación de software cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al reinstalar software: {ex.Message}";
                _log.Error("[Restore] Error reinstalando software", ex);
            }

            return result;
        }

        public async Task<RestoreResult> RestoreUserSettingsAsync(string settingsPath, IProgress<(int Percentage, string Message)> progress, CancellationToken ct = default)
        {
            var result = new RestoreResult();
            try
            {
                if (!Directory.Exists(settingsPath))
                {
                    result.Success = false;
                    result.Message = "La carpeta de configuraciones no existe.";
                    return result;
                }

                result.ItemsRestored = 0;

                // Restaurar configuraciones de AppData
                var appDataFolders = new[] { "AppData_Roaming", "AppData_Local" };
                foreach (var folderName in appDataFolders)
                {
                    var sourceFolder = Path.Combine(settingsPath, folderName);
                    if (!Directory.Exists(sourceFolder))
                        continue;

                    var targetBase = folderName == "AppData_Roaming"
                        ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                        : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    foreach (var appFolder in Directory.EnumerateDirectories(sourceFolder))
                    {
                        ct.ThrowIfCancellationRequested();
                        var appName = new DirectoryInfo(appFolder).Name;
                        var targetFolder = Path.Combine(targetBase, appName);

                        await CopyDirectoryAsync(appFolder, targetFolder, ct);
                        result.ItemsRestored++;
                    }
                }

                // Importar registros
                var registryPath = Path.Combine(settingsPath, "Registry");
                if (Directory.Exists(registryPath))
                {
                    var regFiles = Directory.GetFiles(registryPath, "*.reg");
                    foreach (var regFile in regFiles)
                    {
                        ct.ThrowIfCancellationRequested();
                        var psi = new ProcessStartInfo("regedit", $"/s \"{regFile}\"")
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
                                if (process.ExitCode == 0)
                                    result.ItemsRestored++;
                            }
                        }
                    }
                }

                result.Success = true;
                result.Message = $"{result.ItemsRestored} configuraciones restauradas.";
                _log.Info($"[Restore] Configuraciones restauradas: {result.ItemsRestored}");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Message = "Restauración de configuraciones cancelada.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al restaurar configuraciones: {ex.Message}";
                _log.Error("[Restore] Error restaurando configuraciones", ex);
            }

            return result;
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
    }
}
