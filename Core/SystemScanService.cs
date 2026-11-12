using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public class SystemScanService : ISystemScanService
    {
        private readonly IDriverService _driverService;
        private readonly IDiskHealthService _diskHealthService;
        private readonly IMonitoringService _monitoringService;
        private readonly ITemperatureMonitorService _temperatureMonitorService;
        private readonly IStartupService _startupService;
        private readonly ISystemMaintenanceService _systemMaintenanceService;
        private readonly ISecurityService _securityService;
        private readonly IPrivacyService _privacyService;
        private readonly IBloatwareService _bloatwareService;
        private readonly IRestorePointService _restorePointService;

        public SystemScanService(
            IDriverService driverService,
            IDiskHealthService diskHealthService,
            IMonitoringService monitoringService,
            ITemperatureMonitorService temperatureMonitorService,
            IStartupService startupService,
            ISystemMaintenanceService systemMaintenanceService,
            ISecurityService securityService,
            IPrivacyService privacyService,
            IBloatwareService bloatwareService,
            IRestorePointService restorePointService)
        {
            _driverService = driverService;
            _diskHealthService = diskHealthService;
            _monitoringService = monitoringService;
            _temperatureMonitorService = temperatureMonitorService;
            _startupService = startupService;
            _systemMaintenanceService = systemMaintenanceService;
            _securityService = securityService;
            _privacyService = privacyService;
            _bloatwareService = bloatwareService;
            _restorePointService = restorePointService;
        }

        public async Task<ScanResult> RunFullScanAsync(CancellationToken cancellationToken = default)
        {
            var issues = new List<ScanIssue>();

            // 1. Drivers con problemas
            await ScanDriversAsync(issues, cancellationToken);

            // 2. Salud del disco (SMART)
            await ScanDiskHealthAsync(issues, cancellationToken);

            // 3. Uso de CPU/RAM
            await ScanSystemUsageAsync(issues, cancellationToken);

            // 4. Temperaturas
            await ScanTemperaturesAsync(issues, cancellationToken);

            // 5. Programas de inicio
            await ScanStartupItemsAsync(issues, cancellationToken);

            // 6. Archivos temporales
            await ScanTemporaryFilesAsync(issues, cancellationToken);

            // 7. Seguridad (firewall, antivirus, UAC)
            await ScanSecurityAsync(issues, cancellationToken);

            // 8. Privacidad
            await ScanPrivacyAsync(issues, cancellationToken);

            // 9. Bloatware
            await ScanBloatwareAsync(issues, cancellationToken);

            // 10. Punto de restauración
            await ScanRestorePointAsync(issues, cancellationToken);

            // Calcular score de salud
            var healthScore = CalculateHealthScore(issues);

            // Resumen por categoría
            var categorySummary = issues
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ScanResult
            {
                Timestamp = DateTime.Now,
                Issues = issues,
                HealthScore = healthScore,
                CategorySummary = categorySummary
            };
        }

        private async Task ScanDriversAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var drivers = await _driverService.GetDriversWithProblemsAsync();
                foreach (var driver in drivers)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Drivers,
                        Severity = IssueSeverity.Warning,
                        Title = $"Driver con problema: {driver.Name}",
                        Description = driver.ErrorDescription ?? driver.Status ?? "Estado desconocido",
                        Solution = "Ir a la sección de Drivers para exportar o actualizar este controlador.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Drivers,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar los drivers",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación con permisos de administrador.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanDiskHealthAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var disks = await _diskHealthService.GetDiskHealthAsync();
                foreach (var disk in disks)
                {
                    if (!disk.SmartOk && disk.SmartStatusKnown)
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Disco,
                            Severity = IssueSeverity.Critical,
                            Title = $"Disco en riesgo: {disk.Model}",
                            Description = $"Estado SMART: {disk.SmartStatus}. Capacidad: {disk.Capacity}",
                            Solution = "Realizar backup inmediato de los datos importantes y considerar reemplazar el disco.",
                            IsSolvable = false,
                            IsResolved = false
                        });
                    }
                    else if (disk.SmartStatusKnown && !string.IsNullOrEmpty(disk.SmartStatus))
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Disco,
                            Severity = IssueSeverity.Info,
                            Title = $"Disco: {disk.Model}",
                            Description = $"Estado SMART: {disk.SmartStatus}. Temperatura: {disk.Temperature}°C",
                            Solution = "El disco parece estar en buen estado.",
                            IsSolvable = false,
                            IsResolved = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Disco,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar la salud del disco",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación con permisos de administrador.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanSystemUsageAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var usage = await _monitoringService.GetSystemUsageAsync();

                if (usage.CpuUsage > 90)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Rendimiento,
                        Severity = IssueSeverity.Critical,
                        Title = "Uso de CPU muy alto",
                        Description = $"La CPU está al {usage.CpuUsage:F1}%. Esto puede causar lentitud y sobrecalentamiento.",
                        Solution = "Ir a la sección de Rendimiento para identificar y cerrar los procesos que más consumen.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else if (usage.CpuUsage > 75)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Rendimiento,
                        Severity = IssueSeverity.Warning,
                        Title = "Uso de CPU elevado",
                        Description = $"La CPU está al {usage.CpuUsage:F1}%. Monitorear los procesos activos.",
                        Solution = "Revisar la sección de Rendimiento para ver los procesos más activos.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }

                if (usage.RamUsage > 90)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Rendimiento,
                        Severity = IssueSeverity.Critical,
                        Title = "Uso de RAM muy alto",
                        Description = $"La memoria RAM está al {usage.RamUsage:F1}%. El sistema puede volverse muy lento.",
                        Solution = "Cerrar aplicaciones innecesarias o considerar ampliar la memoria RAM.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else if (usage.RamUsage > 75)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Rendimiento,
                        Severity = IssueSeverity.Warning,
                        Title = "Uso de RAM elevado",
                        Description = $"La memoria RAM está al {usage.RamUsage:F1}%.",
                        Solution = "Cerrar aplicaciones que no estés usando para liberar memoria.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Rendimiento,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar el uso del sistema",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanTemperaturesAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var temp = await _temperatureMonitorService.GetCpuTemperatureCAsync();
                if (temp.HasValue)
                {
                    if (temp.Value > 85)
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Hardware,
                            Severity = IssueSeverity.Critical,
                            Title = "Temperatura de CPU crítica",
                            Description = $"La CPU está a {temp.Value:F1}°C. Riesgo de daño por sobrecalentamiento.",
                            Solution = "Limpiar el equipo de polvo, verificar el ventilador y la pasta térmica.",
                            IsSolvable = false,
                            IsResolved = false
                        });
                    }
                    else if (temp.Value > 75)
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Hardware,
                            Severity = IssueSeverity.Warning,
                            Title = "Temperatura de CPU elevada",
                            Description = $"La CPU está a {temp.Value:F1}°C. Monitorear para evitar sobrecalentamiento.",
                            Solution = "Verificar que el ventilador funcione correctamente y mejorar la ventilación.",
                            IsSolvable = false,
                            IsResolved = false
                        });
                    }
                }
            }
            catch
            {
                // La temperatura puede no estar disponible en todos los equipos
            }
        }

        private async Task ScanStartupItemsAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var items = await _startupService.GetStartupItemsAsync();
                var enabledItems = items.Where(i => i.IsEnabled).ToList();

                if (enabledItems.Count >= 10)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Inicio,
                        Severity = IssueSeverity.Warning,
                        Title = $"Demasiados programas de inicio ({enabledItems.Count})",
                        Description = $"Tienes {enabledItems.Count} programas que se inician con Windows. Esto ralentiza el arranque.",
                        Solution = "Deshabilitar los programas de inicio que no necesitas en la sección de Inicio.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else if (enabledItems.Count >= 5)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Inicio,
                        Severity = IssueSeverity.Info,
                        Title = $"Programas de inicio ({enabledItems.Count})",
                        Description = $"Tienes {enabledItems.Count} programas que se inician con Windows.",
                        Solution = "Considera deshabilitar los que no necesitas para un arranque más rápido.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Inicio,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar los programas de inicio",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación con permisos de administrador.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanTemporaryFilesAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var options = new CleaningOptions
                {
                    CleanSystemTemp = true,
                    CleanRecycleBin = false,
                    CleanBrowserCache = false,
                    CleanWindowsUpdate = false,
                    CleanThumbnails = false,
                    CleanEventLogs = false
                };
                var candidates = await _systemMaintenanceService.PreviewTemporaryFilesAsync(options, ct);
                var totalSize = candidates.Sum(c => c.SizeBytes);

                if (totalSize > 5_000_000_000) // 5 GB
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Sistema,
                        Severity = IssueSeverity.Warning,
                        Title = $"Archivos temporales: {FormatBytes(totalSize)}",
                        Description = $"Hay {candidates.Count} archivos temporales que ocupan {FormatBytes(totalSize)}. Liberar espacio mejora el rendimiento.",
                        Solution = "Ejecutar la limpieza de archivos temporales en la sección de Mantenimiento.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else if (totalSize > 1_000_000_000) // 1 GB
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Sistema,
                        Severity = IssueSeverity.Info,
                        Title = $"Archivos temporales: {FormatBytes(totalSize)}",
                        Description = $"Hay {candidates.Count} archivos temporales que ocupan {FormatBytes(totalSize)}.",
                        Solution = "Puedes limpiarlos en la sección de Mantenimiento si necesitas espacio.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Sistema,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar los archivos temporales",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación con permisos de administrador.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanSecurityAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var status = await _securityService.GetSecurityStatusAsync();

                if (!status.IsAntivirusEnabled)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Seguridad,
                        Severity = IssueSeverity.Critical,
                        Title = "Antivirus desactivado o no detectado",
                        Description = "No se encontró un antivirus activo. Tu sistema está vulnerable a amenazas.",
                        Solution = "Activar Windows Defender o instalar un antivirus confiable.",
                        IsSolvable = false,
                        IsResolved = false
                    });
                }

                if (!status.IsFirewallEnabled)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Seguridad,
                        Severity = IssueSeverity.Critical,
                        Title = "Firewall desactivado",
                        Description = "El firewall de Windows está desactivado. Tu red es vulnerable.",
                        Solution = "Activar el firewall desde el Panel de control de Windows.",
                        IsSolvable = false,
                        IsResolved = false
                    });
                }

                if (!status.IsUacEnabled)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Seguridad,
                        Severity = IssueSeverity.Warning,
                        Title = "UAC desactivado",
                        Description = "El Control de cuentas de usuario (UAC) está desactivado.",
                        Solution = "Activar UAC desde el Panel de control para mayor seguridad.",
                        IsSolvable = false,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Seguridad,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar la seguridad",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación con permisos de administrador.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanPrivacyAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var settings = await _privacyService.GetPrivacySettingsAsync();
                var invasiveSettings = settings.Where(s => s.CurrentValue && !s.RecommendedValue).ToList();

                if (invasiveSettings.Count >= 3)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Privacidad,
                        Severity = IssueSeverity.Warning,
                        Title = $"Configuraciones de privacidad invasivas ({invasiveSettings.Count})",
                        Description = $"Hay {invasiveSettings.Count} configuraciones que recopilan datos personales.",
                        Solution = "Revisar y desactivar las configuraciones de privacidad en la sección de Privacidad.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else if (invasiveSettings.Count > 0)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Privacidad,
                        Severity = IssueSeverity.Info,
                        Title = $"Configuraciones de privacidad ({invasiveSettings.Count})",
                        Description = $"Hay {invasiveSettings.Count} configuraciones que recopilan datos.",
                        Solution = "Revisar la sección de Privacidad para ajustar estas configuraciones.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Privacidad,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar la privacidad",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanBloatwareAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var bloatware = await _bloatwareService.GetBloatwareAppsAsync();
                var bloatwareList = bloatware.ToList();

                if (bloatwareList.Count >= 5)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Sistema,
                        Severity = IssueSeverity.Warning,
                        Title = $"Bloatware detectado ({bloatwareList.Count} apps)",
                        Description = $"Se encontraron {bloatwareList.Count} aplicaciones preinstaladas innecesarias que consumen recursos.",
                        Solution = "Desinstalar el bloatware en la sección de Aplicaciones.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else if (bloatwareList.Count > 0)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Sistema,
                        Severity = IssueSeverity.Info,
                        Title = $"Bloatware detectado ({bloatwareList.Count} apps)",
                        Description = $"Se encontraron {bloatwareList.Count} aplicaciones preinstaladas innecesarias.",
                        Solution = "Puedes desinstalarlas en la sección de Aplicaciones si lo deseas.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Sistema,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar el bloatware",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private async Task ScanRestorePointAsync(List<ScanIssue> issues, CancellationToken ct)
        {
            try
            {
                var lastPoint = await _restorePointService.GetLastRestorePointAsync();

                if (lastPoint.Date == null)
                {
                    issues.Add(new ScanIssue
                    {
                        Category = ScanCategory.Sistema,
                        Severity = IssueSeverity.Warning,
                        Title = "Sin punto de restauración",
                        Description = "No se encontró ningún punto de restauración. Si algo falla, no podrás revertir los cambios.",
                        Solution = "Crear un punto de restauración en la sección de Reparación.",
                        IsSolvable = true,
                        IsResolved = false
                    });
                }
                else
                {
                    var daysOld = (DateTime.Now - lastPoint.Date.Value).TotalDays;
                    if (daysOld > 7)
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Sistema,
                            Severity = IssueSeverity.Warning,
                            Title = $"Punto de restauración antiguo ({daysOld:F0} días)",
                            Description = $"El último punto de restauración es de {lastPoint.Name} hace {daysOld:F0} días.",
                            Solution = "Crear un nuevo punto de restauración en la sección de Reparación.",
                            IsSolvable = true,
                            IsResolved = false
                        });
                    }
                    else
                    {
                        issues.Add(new ScanIssue
                        {
                            Category = ScanCategory.Sistema,
                            Severity = IssueSeverity.Info,
                            Title = $"Punto de restauración reciente ({daysOld:F0} días)",
                            Description = $"El último punto de restauración es de {lastPoint.Name}.",
                            Solution = "El sistema tiene un punto de restauración reciente.",
                            IsSolvable = false,
                            IsResolved = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ScanIssue
                {
                    Category = ScanCategory.Sistema,
                    Severity = IssueSeverity.Info,
                    Title = "No se pudo verificar el punto de restauración",
                    Description = ex.Message,
                    Solution = "Reiniciar la aplicación con permisos de administrador.",
                    IsSolvable = false,
                    IsResolved = false
                });
            }
        }

        private int CalculateHealthScore(List<ScanIssue> issues)
        {
            if (issues.Count == 0)
                return 100;

            int score = 100;

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

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:F1} {sizes[order]}";
        }
    }
}
