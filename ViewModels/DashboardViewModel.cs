using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class DashboardViewModel : ViewModelBase, IDisposable
    {
        private readonly IMonitoringService _monitoringService;
        private readonly ITemperatureMonitorService _temperatureService;
        private readonly IBatteryService _batteryService;
        private readonly IDiskHealthService _diskHealthService;
        private readonly ISecurityService _securityService;
        private readonly ISystemInfoService _systemInfoService;
        private readonly IStartupService _startupService;
        private readonly IStorageScanService _storageScanService;
        private readonly ISystemScanService _systemScanService;
        private CancellationTokenSource? _monitoringCts;
        private bool _disposed;

        private double _cpuUsage;
        private double _ramUsage;
        private double? _cpuTempC;
        private string _cpuTempStatus = "No disponible";
        private string? _cpuTempText;
        private string? _diskUsageText;
        private BatteryInfo _batteryInfo = new();
        private string _statusMessage = "Cargando dashboard...";
        private bool _isBusy;
        private SecurityStatus _securityStatus = new();
        private SystemInfo? _systemInfo;

        // Saludo contextual
        private string _greetingMessage = "Bienvenido";

        // Score de salud
        private int _healthScore = 100;
        private int _healthChecksOk;
        private int _healthWarnings;
        private int _healthCritical;

        public double CpuUsage { get => _cpuUsage; private set => SetProperty(ref _cpuUsage, value); }
        public double RamUsage { get => _ramUsage; private set => SetProperty(ref _ramUsage, value); }
        public double? CpuTempC { get => _cpuTempC; private set => SetProperty(ref _cpuTempC, value); }
        public string CpuTempStatus { get => _cpuTempStatus; private set => SetProperty(ref _cpuTempStatus, value); }
        public string? CpuTempText { get => _cpuTempText; private set => SetProperty(ref _cpuTempText, value); }
        public string? DiskUsageText { get => _diskUsageText; private set => SetProperty(ref _diskUsageText, value); }
        public BatteryInfo BatteryInfo { get => _batteryInfo; private set => SetProperty(ref _batteryInfo, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public SecurityStatus SecurityStatus { get => _securityStatus; private set => SetProperty(ref _securityStatus, value); }
        public SystemInfo? SystemInfo { get => _systemInfo; private set => SetProperty(ref _systemInfo, value); }
        public ObservableCollection<DiskHealthInfo> Disks { get; } = new();

        public string GreetingMessage { get => _greetingMessage; private set => SetProperty(ref _greetingMessage, value); }
        public int HealthScore { get => _healthScore; private set => SetProperty(ref _healthScore, value); }
        public int HealthChecksOk { get => _healthChecksOk; private set => SetProperty(ref _healthChecksOk, value); }
        public int HealthWarnings { get => _healthWarnings; private set => SetProperty(ref _healthWarnings, value); }
        public int HealthCritical { get => _healthCritical; private set => SetProperty(ref _healthCritical, value); }

        // Alertas dinámicas
        public ObservableCollection<DashboardAlert> Alerts { get; } = new();

        // Actividad reciente
        public ObservableCollection<ActivityItem> RecentActivities { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand RefreshSecurityCommand { get; }
        public ICommand QuickScanCommand { get; }

        public DashboardViewModel(
            IMonitoringService monitoringService,
            ITemperatureMonitorService temperatureService,
            IBatteryService batteryService,
            IDiskHealthService diskHealthService,
            ISecurityService securityService,
            ISystemInfoService systemInfoService,
            IStartupService startupService,
            IStorageScanService storageScanService,
            ISystemScanService systemScanService)
        {
            _monitoringService = monitoringService;
            _temperatureService = temperatureService;
            _batteryService = batteryService;
            _diskHealthService = diskHealthService;
            _securityService = securityService;
            _systemInfoService = systemInfoService;
            _startupService = startupService;
            _storageScanService = storageScanService;
            _systemScanService = systemScanService;

            RefreshCommand = new AsyncRelayCommand(_ => RefreshAllAsync());
            RefreshSecurityCommand = new AsyncRelayCommand(_ => LoadSecurityStatusAsync());
            QuickScanCommand = new AsyncRelayCommand(_ => RunQuickScanAsync());
        }

        public async Task OnNavigatedToAsync()
        {
            await _monitoringService.StartAsync();
            CalculateGreeting();
            await RefreshAllAsync();
            StartLiveMonitoring();
        }

        private void CalculateGreeting()
        {
            var hour = DateTime.Now.Hour;
            var userName = Environment.UserName;
            if (hour < 12)
                GreetingMessage = $"Buenos días, {userName}";
            else if (hour < 19)
                GreetingMessage = $"Buenas tardes, {userName}";
            else
                GreetingMessage = $"Buenas noches, {userName}";
        }

        public async Task OnNavigatedFromAsync()
        {
            StopLiveMonitoring();
            await _monitoringService.StopAsync();
        }

        public async Task RefreshAllAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Actualizando dashboard...";

            try
            {
                SystemInfo = await _systemInfoService.GetSystemInfoAsync();

                // CPU y RAM
                var usage = await _monitoringService.GetSystemUsageAsync();
                CpuUsage = usage.CpuUsage;
                RamUsage = usage.RamUsage;

                // Temperatura
                try
                {
                    var temp = await _temperatureService.GetCpuTemperatureCAsync();
                    if (temp.HasValue)
                    {
                        CpuTempC = temp.Value;
                        CpuTempStatus = temp.Value < 60 ? "Normal" : temp.Value < 80 ? "Elevado" : "¡Alto!";
                        CpuTempText = $"{temp.Value:F0}°C";
                    }
                    else
                    {
                        CpuTempStatus = "No disponible";
                        CpuTempText = null;
                    }
                }
                catch { CpuTempStatus = "No disponible"; CpuTempText = null; }

                // Disco - uso del volumen principal
                try
                {
                    var volumes = _storageScanService.GetVolumes();
                    if (volumes.Count > 0)
                    {
                        var sysVolume = volumes[0];
                        if (sysVolume.TotalBytes > 0)
                        {
                            var pct = (double)sysVolume.UsedBytes / sysVolume.TotalBytes * 100;
                            DiskUsageText = $"{pct:F0}%";
                        }
                    }
                }
                catch { DiskUsageText = null; }

                // Batería
                var battery = await _batteryService.GetBatteryStatusAsync();
                if (battery.IsReadSuccessful)
                    BatteryInfo = battery;

                // Discos
                Disks.Clear();
                var disks = await _diskHealthService.GetDiskHealthAsync();
                foreach (var disk in disks) Disks.Add(disk);

                // Seguridad
                await LoadSecurityStatusAsync();

                // Score de salud y alertas
                await CalculateHealthScoreAndAlertsAsync();

                // Actividad reciente (placeholder con la última actualización)
                UpdateRecentActivities();

                StatusMessage = $"Última actualización: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CalculateHealthScoreAndAlertsAsync()
        {
            Alerts.Clear();
            var ok = 0;
            var warnings = 0;
            var criticals = 0;

            // Check 1: CPU usage
            if (CpuUsage > 90)
            {
                criticals++;
                Alerts.Add(new DashboardAlert
                {
                    Severity = IssueSeverity.Critical,
                    Title = "Uso de CPU muy alto",
                    Description = $"La CPU está al {CpuUsage:F0}%, lo que puede indicar un proceso problemático.",
                    ActionLabel = "Ver procesos",
                    NavigateTo = "Hardware"
                });
            }
            else if (CpuUsage > 70)
            {
                warnings++;
                Alerts.Add(new DashboardAlert
                {
                    Severity = IssueSeverity.Warning,
                    Title = "Uso de CPU elevado",
                    Description = $"La CPU está al {CpuUsage:F0}%. Considera cerrar aplicaciones innecesarias.",
                    ActionLabel = "Ver procesos",
                    NavigateTo = "Hardware"
                });
            }
            else ok++;

            // Check 2: RAM usage
            if (RamUsage > 90)
            {
                criticals++;
                Alerts.Add(new DashboardAlert
                {
                    Severity = IssueSeverity.Critical,
                    Title = "Memoria RAM casi agotada",
                    Description = $"La RAM está al {RamUsage:F0}%. Cierra aplicaciones para liberar memoria.",
                    ActionLabel = "Ver procesos",
                    NavigateTo = "Hardware"
                });
            }
            else if (RamUsage > 70)
            {
                warnings++;
            }
            else ok++;

            // Check 3: Disk usage
            try
            {
                var volumes = _storageScanService.GetVolumes();
                if (volumes.Count > 0)
                {
                    var sysVolume = volumes[0];
                    if (sysVolume.TotalBytes > 0)
                    {
                        var pct = (double)sysVolume.UsedBytes / sysVolume.TotalBytes * 100;
                        if (pct > 90)
                        {
                            criticals++;
                            Alerts.Add(new DashboardAlert
                            {
                                Severity = IssueSeverity.Critical,
                                Title = "Disco casi lleno",
                                Description = $"El disco tiene un {pct:F0}% de uso. Necesitas liberar espacio.",
                                ActionLabel = "Liberar espacio",
                                NavigateTo = "Limpieza"
                            });
                        }
                        else if (pct > 80)
                        {
                            warnings++;
                            Alerts.Add(new DashboardAlert
                            {
                                Severity = IssueSeverity.Warning,
                                Title = "Disco con alto uso",
                                Description = $"El disco tiene un {pct:F0}% de uso. Considera limpiar archivos innecesarios.",
                                ActionLabel = "Liberar espacio",
                                NavigateTo = "Limpieza"
                            });
                        }
                        else ok++;
                    }
                }
            }
            catch { ok++; }

            // Check 4: Temperature
            if (CpuTempC.HasValue && CpuTempC.Value > 85)
            {
                criticals++;
                Alerts.Add(new DashboardAlert
                {
                    Severity = IssueSeverity.Critical,
                    Title = "Temperatura CPU crítica",
                    Description = $"La CPU está a {CpuTempC.Value:F0}°C. Verifica la ventilación.",
                    NavigateTo = null
                });
            }
            else if (CpuTempC.HasValue && CpuTempC.Value > 70)
            {
                warnings++;
            }
            else if (CpuTempC.HasValue) ok++;

            // Check 5: Startup items
            try
            {
                var startupItems = await _startupService.GetStartupItemsAsync();
                var enabledCount = startupItems.Count(item => item.IsEnabled);
                if (enabledCount > 8)
                {
                    warnings++;
                    Alerts.Add(new DashboardAlert
                    {
                        Severity = IssueSeverity.Warning,
                        Title = $"{enabledCount} programas arrancan con Windows",
                        Description = "Demasiados programas de inicio pueden ralentizar el arranque.",
                        ActionLabel = "Gestionar inicio",
                        NavigateTo = "InicioWindows"
                    });
                }
                else ok++;
            }
            catch { ok++; }

            // Check 6: Disk health (SMART)
            var hasCriticalDisk = Disks.Any(d => d.SmartStatus.Contains("Caution", StringComparison.OrdinalIgnoreCase)
                                             || d.SmartStatus.Contains("Bad", StringComparison.OrdinalIgnoreCase)
                                             || d.SmartStatus.Contains("Critical", StringComparison.OrdinalIgnoreCase));
            if (hasCriticalDisk)
            {
                criticals++;
                Alerts.Add(new DashboardAlert
                {
                    Severity = IssueSeverity.Critical,
                    Title = "Disco con problemas detectados",
                    Description = "Uno o más discos muestran estado SMART preocupante. Haz un backup inmediatamente.",
                    ActionLabel = "Ver discos",
                    NavigateTo = "SaludDiscos"
                });
            }
            else if (Disks.Count > 0) ok++;

            // Calculate score
            var totalChecks = ok + warnings + criticals;
            HealthScore = totalChecks > 0 ? (int)((double)ok / totalChecks * 100) : 100;
            HealthChecksOk = ok;
            HealthWarnings = warnings;
            HealthCritical = criticals;
        }

        private void UpdateRecentActivities()
        {
            // Add current refresh as activity
            RecentActivities.Insert(0, new ActivityItem
            {
                Timestamp = DateTime.Now,
                Title = "Dashboard actualizado",
                Description = "Métricas y alertas refrescadas",
                StatusIcon = "🔄"
            });

            // Keep only last 5
            while (RecentActivities.Count > 5)
                RecentActivities.RemoveAt(RecentActivities.Count - 1);
        }

        private async Task RunQuickScanAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Ejecutando escáner rápido...";

            try
            {
                var result = await _systemScanService.RunFullScanAsync();
                HealthScore = result.HealthScore;

                var ok = result.Issues.Count(i => i.Severity == IssueSeverity.Info || i.IsResolved);
                var warnings = result.Issues.Count(i => i.Severity == IssueSeverity.Warning && !i.IsResolved);
                var criticals = result.Issues.Count(i => i.Severity == IssueSeverity.Critical && !i.IsResolved);

                HealthChecksOk = ok;
                HealthWarnings = warnings;
                HealthCritical = criticals;

                // Update alerts from scan
                Alerts.Clear();
                foreach (var issue in result.Issues.Take(5))
                {
                    if (!issue.IsResolved)
                    {
                        Alerts.Add(new DashboardAlert
                        {
                            Severity = issue.Severity,
                            Title = issue.Title,
                            Description = issue.Description,
                            ActionLabel = issue.IsSolvable ? "Resolver" : null,
                            NavigateTo = "RevisarPC"
                        });
                    }
                }

                RecentActivities.Insert(0, new ActivityItem
                {
                    Timestamp = DateTime.Now,
                    Title = "Escáner completado",
                    Description = $"Score: {HealthScore}/100",
                    StatusIcon = HealthScore >= 70 ? "✅" : "⚠️"
                });

                while (RecentActivities.Count > 5)
                    RecentActivities.RemoveAt(RecentActivities.Count - 1);

                StatusMessage = $"Escáner completado — Score: {HealthScore}/100";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error en escáner: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadSecurityStatusAsync()
        {
            try
            {
                var security = await _securityService.GetSecurityStatusAsync();
                SecurityStatus = security;
            }
            catch { /* Silenciar errores de seguridad */ }
        }

        private void StartLiveMonitoring()
        {
            StopLiveMonitoring();
            _monitoringCts = new CancellationTokenSource();
            var token = _monitoringCts.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, token);
                        var snap = await _monitoringService.GetGlobalUsageAsync(token);
                        if (snap.CpuUsage.HasValue) CpuUsage = snap.CpuUsage.Value;
                        if (snap.RamUsage.HasValue) RamUsage = snap.RamUsage.Value;
                    }
                    catch (OperationCanceledException) { break; }
                    catch { /* Silenciar errores de monitoreo */ }
                }
            }, token);
        }

        private void StopLiveMonitoring()
        {
            _monitoringCts?.Cancel();
            _monitoringCts?.Dispose();
            _monitoringCts = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopLiveMonitoring();
        }
    }
}
