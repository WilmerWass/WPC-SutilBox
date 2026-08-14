using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

#nullable enable

namespace Wpc_SutilBox.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // ==========================================
        // 1. CAMPOS Y DEPENDENCIAS PRIVADAS
        // ==========================================
        private readonly ITemperatureMonitorService? _temperatureMonitorService;
        private readonly IDiskHealthService? _diskHealthService;
        private readonly IMonitoringService? _monitoringService;
        private readonly IBatteryService? _batteryService;
        private readonly IProcessManagerService? _processManagerService;
        private readonly IRestorePointService? _restorePointService;
        private readonly IPerformanceProfileService? _performanceProfileService;
        private readonly IStartupService? _startupService;
        private readonly IBloatwareService? _bloatwareService;
        private readonly IWingetService? _wingetService;
        private readonly ISettingsService? _settingsService;
        private readonly ILogService? _logService;
        private readonly ILocalizationService? _localizationService;
        private readonly ProfileEditorViewModel? _profileEditorViewModel;
        private CancellationTokenSource? _monitoringCts;
        private Task? _monitoringTask;
        private bool _settingsReady;
        private DateTime _lastIdleOptimization = DateTime.MinValue;
        private DateTime _lastGlobalUsageSampleUtc = DateTime.MinValue;
        private DateTime _lastTemperatureSampleUtc = DateTime.MinValue;
        private DateTime _lastBatterySampleUtc = DateTime.MinValue;

        private static readonly TimeSpan GlobalUsageInterval = TimeSpan.FromSeconds(2.5);
        private static readonly TimeSpan TemperatureInterval = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan BatteryInterval = TimeSpan.FromSeconds(45);

        private int _activeTcpConnections;
        private string _systemStatus = string.Empty;
        private string _applicationTitle = string.Empty;
        private string _currentSection = "Dashboard";
        private double _cpuUsage;
        private double _ramUsage;
        private double? _cpuTempC;
        private string _cpuTemperatureStatus = "No disponible";
        private BatteryInfo _batteryInfo = new();
        private PerformanceMode _currentMode = PerformanceMode.General;
        private string _welcomeMessage = "Controla y optimiza tu equipo desde un solo lugar.";
        private string _generalStatusMessage = "Sistema listo.";
        private bool _runOnStartup;
        private bool _optimizeOnIdle;
        private bool _minimizeToTray = true;
        private bool _isDarkMode = true;
        private string _selectedLanguage = "es";
        private string _accentColor = "#3B82F6";
        private string _processSearchText = string.Empty;
        private double _netRecvMbps;
        private double _diskWritesPerSec;

        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // ==========================================
        // 2. PROPIEDADES DE ENLACE (BINDINGS)
        // ==========================================
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public double CpuUsage { get => _cpuUsage; set => SetProperty(ref _cpuUsage, value); }
        public double RamUsage { get => _ramUsage; set => SetProperty(ref _ramUsage, value); }
        public double? CpuTempC { get => _cpuTempC; private set => SetProperty(ref _cpuTempC, value); }
        public string CpuTemperatureStatus { get => _cpuTemperatureStatus; private set => SetProperty(ref _cpuTemperatureStatus, value); }
        public BatteryInfo BatteryInfo { get => _batteryInfo; private set => SetProperty(ref _batteryInfo, value); }
        public PerformanceMode CurrentMode { get => _currentMode; set => SetProperty(ref _currentMode, value); }
        public string WelcomeMessage { get => _welcomeMessage; private set => SetProperty(ref _welcomeMessage, value); }
        public string GeneralStatusMessage { get => _generalStatusMessage; private set => SetProperty(ref _generalStatusMessage, value); }
        public bool RunOnStartup { get => _runOnStartup; set => SetSetting(ref _runOnStartup, value); }
        public bool OptimizeOnIdle { get => _optimizeOnIdle; set => SetSetting(ref _optimizeOnIdle, value); }
        public bool MinimizeToTray { get => _minimizeToTray; set => SetSetting(ref _minimizeToTray, value); }
        public bool IsDarkMode { get => _isDarkMode; set { if (SetProperty(ref _isDarkMode, value) && _settingsReady) { if (Application.Current is App app) app.ChangeTheme(value); _ = SaveSettingsAsync(); } } }
        public string SelectedLanguage { get => _selectedLanguage; set => SetSetting(ref _selectedLanguage, value); }
        public string AccentColor { get => _accentColor; private set => SetProperty(ref _accentColor, value); }
        public double NetRecvMbps { get => _netRecvMbps; private set => SetProperty(ref _netRecvMbps, value); }
        public double DiskWritesPerSec { get => _diskWritesPerSec; private set => SetProperty(ref _diskWritesPerSec, value); }
        public ProfileEditorViewModel? ProfileEditorViewModel => _profileEditorViewModel;

        public ObservableCollection<StartupItem> StartupItems { get; } = new();
        public ObservableCollection<BloatwareApp> BloatwareApps { get; } = new();
        public ObservableCollection<WingetApp> UpdatableApps { get; } = new();
        public ObservableCollection<string> RecentLogEntries { get; } = new();
        public ObservableCollection<CpuCoreMetric> CpuPerCore { get; } = new();
        public ObservableCollection<ProcessInfoDto> Processes { get; } = new();
        public string ProcessSearchText { get => _processSearchText; set => SetProperty(ref _processSearchText, value); }
        // Nuevos Perfiles de Rendimiento
        private bool _isEquilibradoModeSelected = true; // Por defecto
        public bool IsEquilibradoModeSelected
        {
            get => _isEquilibradoModeSelected;
            set { SetProperty(ref _isEquilibradoModeSelected, value); if (value) OnPerformanceModeChanged("Equilibrado"); }
        }
        private bool _isGamingModeSelected;
        public bool IsGamingModeSelected
        {
            get => _isGamingModeSelected;
            set { SetProperty(ref _isGamingModeSelected, value); if (value) OnPerformanceModeChanged("Gaming"); }
        }
        private bool _isProductividadModeSelected;
        public bool IsProductividadModeSelected
        {
            get => _isProductividadModeSelected;
            set { SetProperty(ref _isProductividadModeSelected, value); if (value) OnPerformanceModeChanged("Productividad"); }
        }
        private bool _isDesarrolloModeSelected;
        public bool IsDesarrolloModeSelected
        {
            get => _isDesarrolloModeSelected;
            set { SetProperty(ref _isDesarrolloModeSelected, value); if (value) OnPerformanceModeChanged("Desarrollo"); }
        }
        private bool _isCustomModeSelected;
        public bool IsCustomModeSelected
        {
            get => _isCustomModeSelected;
            set { SetProperty(ref _isCustomModeSelected, value); if (value) OnPerformanceModeChanged("A tu medida"); }
        }
        private void OnPerformanceModeChanged(string modeName)
        {
            Debug.WriteLine($"Modo de rendimiento cambiado a: {modeName}");
            WriteLog($"El usuario seleccionó el perfil de rendimiento: {modeName}");
        }
        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }

        public string CurrentSection
        {
            get => _currentSection;
            set => SetProperty(ref _currentSection, value);
        }

        public int ActiveTcpConnections
        {
            get => _activeTcpConnections;
            set => SetProperty(ref _activeTcpConnections, value);
        }

        public string SystemStatus
        {
            get => _systemStatus;
            set => SetProperty(ref _systemStatus, value);
        }

        public ObservableCollection<DiskHealthInfo> Disks { get; } = new ObservableCollection<DiskHealthInfo>();
        public bool IsWindowVisible { get; set; } = true;

        private async Task MiAccionAsync()
{
    IsBusy = true;
    StatusMessage = "Optimizando sistema...";

    try
    {
        // Tu código de proceso asíncrono aquí
        await Task.Delay(2000);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        // ESTO APAGA LA CARGA INFINITA OBLIGATORIAMENTE
        IsBusy = false;
        StatusMessage = string.Empty;
    }
}

        // ==========================================
        // 3. COMANDOS DE ACCIÓN Y NAVEGACIÓN
        // ==========================================
        public ICommand OpenDiscordCommand { get; }
        public ICommand OpenCleanmgrCommand { get; }
        public ICommand CleanTempFilesCommand { get; }
        public ICommand OpenLogFolderCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand RunToolCommand { get; }
        public ICommand RunSfcCommand { get; }
        public ICommand RunDismCommand { get; }
        public ICommand RunChkdskCommand { get; }

        // Comandos requeridos por App.xaml.cs y MainWindow.xaml
        public ICommand? PcBoostCommand { get; set; }
        public ICommand? OptimizeRamCommand { get; set; }
        public ICommand QuickOptimizeCommand { get; }
        public ICommand ApplyPerformanceModeCommand { get; }
        public ICommand CreateRestorePointCommand { get; }
        public ICommand RefreshStartupItemsCommand { get; }
        public ICommand EnableStartupItemCommand { get; }
        public ICommand DisableStartupItemCommand { get; }
        public ICommand RefreshBloatwareAppsCommand { get; }
        public ICommand UninstallBloatwareAppCommand { get; }
        public ICommand RefreshUpdatableAppsCommand { get; }
        public ICommand UpdateAllAppsCommand { get; }
        public ICommand UpdateAppCommand { get; }
        public ICommand ChangeAccentColorCommand { get; }
        public ICommand RefreshLogsCommand { get; }
        public ICommand RefreshProcessesCommand { get; }
        public ICommand KillProcessCommand { get; }
        public ICommand ClearProcessSearchCommand { get; }

        // ==========================================
        // 4. CONSTRUCTOR PRINCIPAL
        // ==========================================
        public MainViewModel(
            ITemperatureMonitorService? temperatureMonitorService = null,
            IDiskHealthService? diskHealthService = null,
            IMonitoringService? monitoringService = null,
            IBatteryService? batteryService = null,
            IProcessManagerService? processManagerService = null,
            IRestorePointService? restorePointService = null,
            IPerformanceProfileService? performanceProfileService = null,
            IStartupService? startupService = null,
            IBloatwareService? bloatwareService = null,
            IWingetService? wingetService = null,
            ISettingsService? settingsService = null,
            ILogService? logService = null,
            ProfileEditorViewModel? profileEditorViewModel = null,
            ILocalizationService? localizationService = null)
        {
            _temperatureMonitorService = temperatureMonitorService;
            _diskHealthService = diskHealthService;
            _monitoringService = monitoringService;
            _batteryService = batteryService;
            _processManagerService = processManagerService;
            _restorePointService = restorePointService;
            _performanceProfileService = performanceProfileService;
            _startupService = startupService;
            _bloatwareService = bloatwareService;
            _wingetService = wingetService;
            _settingsService = settingsService;
            _logService = logService;
            _profileEditorViewModel = profileEditorViewModel;
            _localizationService = localizationService;

            ApplicationTitle = "WPC-SutilBox";
            SystemStatus = "WPC-SutilBox - Listo";

            CurrentView = this;
            OpenDiscordCommand = new RelayCommand(ExecuteOpenDiscord);
            OpenCleanmgrCommand = new RelayCommand(ExecuteOpenCleanmgr);
            CleanTempFilesCommand = new AsyncRelayCommand(ExecuteCleanTempFilesAsync);
            OpenLogFolderCommand = new RelayCommand(ExecuteOpenLogFolder);
            RunToolCommand = new RelayCommand(ExecuteRunTool);
            RunSfcCommand = new AsyncRelayCommand(ExecuteRunSfcAsync);
            RunDismCommand = new AsyncRelayCommand(ExecuteRunDismAsync);
            RunChkdskCommand = new RelayCommand(ExecuteRunChkdsk);
            OptimizeRamCommand = new AsyncRelayCommand(ExecuteOptimizeRamAsync);
            CreateRestorePointCommand = new AsyncRelayCommand(ExecuteCreateRestorePointAsync);
            ApplyPerformanceModeCommand = new AsyncRelayCommand(ExecuteApplyPerformanceModeAsync);
            PcBoostCommand = new AsyncRelayCommand(_ => ExecuteApplyPerformanceModeAsync(PerformanceMode.Gamer));
            QuickOptimizeCommand = new AsyncRelayCommand(ExecuteQuickOptimizeAsync);
            RefreshStartupItemsCommand = new AsyncRelayCommand(RefreshStartupItemsAsync);
            EnableStartupItemCommand = new AsyncRelayCommand(async p => await ChangeStartupItemAsync(p, true));
            DisableStartupItemCommand = new AsyncRelayCommand(async p => await ChangeStartupItemAsync(p, false));
            RefreshBloatwareAppsCommand = new AsyncRelayCommand(RefreshBloatwareAppsAsync);
            UninstallBloatwareAppCommand = new AsyncRelayCommand(UninstallBloatwareAppAsync);
            RefreshUpdatableAppsCommand = new AsyncRelayCommand(RefreshUpdatableAppsAsync);
            UpdateAllAppsCommand = new AsyncRelayCommand(UpdateAllAppsAsync);
            UpdateAppCommand = new AsyncRelayCommand(UpdateAppAsync);
            ChangeAccentColorCommand = new RelayCommand(p => ChangeAccentColor(p as string));
            RefreshLogsCommand = new RelayCommand(_ => RefreshLogs());
            RefreshProcessesCommand = new AsyncRelayCommand(RefreshProcessesAsync);
            KillProcessCommand = new AsyncRelayCommand(KillProcessAsync);
            ClearProcessSearchCommand = new RelayCommand(_ => ProcessSearchText = string.Empty);
            _ = LoadSettingsAsync();

            NavigateCommand = new RelayCommand(param =>
            {
                if (param is string section && !string.IsNullOrWhiteSpace(section))
                {
                    CurrentSection = section;
                    WriteLog($"Navegando a la sección: {section}");
                    if (section == "Aplicaciones")
                    {
                        _ = RefreshStartupItemsAsync();
                        _ = RefreshBloatwareAppsAsync();
                    }
                    else if (section == "Historial") RefreshLogs();
                }
            });

            // Escribe un log de prueba al iniciar la app para verificar la ruta
            WriteLog("Aplicación iniciada correctamente en modo Debug.");
        }

        private async Task LoadSettingsAsync()
        {
            if (_settingsService == null) return;
            var settings = await _settingsService.LoadAsync();
            _runOnStartup = settings.RunOnStartup; _optimizeOnIdle = settings.OptimizeOnIdle;
            _minimizeToTray = settings.MinimizeToTray; _isDarkMode = settings.IsDarkMode; _selectedLanguage = settings.Language; _accentColor = settings.AccentColor;
            OnPropertyChanged(nameof(RunOnStartup)); OnPropertyChanged(nameof(OptimizeOnIdle)); OnPropertyChanged(nameof(MinimizeToTray));
            OnPropertyChanged(nameof(IsDarkMode)); OnPropertyChanged(nameof(SelectedLanguage)); OnPropertyChanged(nameof(AccentColor));
            _settingsReady = true;
            RefreshLogs();
        }

        private void SetSetting<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (SetProperty(ref field, value, propertyName) && _settingsReady) _ = SaveSettingsAsync();
        }

        private async Task SaveSettingsAsync()
        {
            if (_settingsService == null) return;
            var settings = await _settingsService.LoadAsync();
            settings.RunOnStartup = RunOnStartup; settings.OptimizeOnIdle = OptimizeOnIdle;
            settings.MinimizeToTray = MinimizeToTray; settings.IsDarkMode = IsDarkMode; settings.Language = SelectedLanguage;
            settings.AccentColor = AccentColor;
            if (_localizationService != null && _localizationService.CurrentLanguage != SelectedLanguage)
                await _localizationService.SetLanguageAsync(SelectedLanguage);
            await _settingsService.SaveAsync(settings);
            ApplyStartupRegistration(RunOnStartup);
        }

        private async Task RefreshStartupItemsAsync()
        {
            if (_startupService == null) return;
            var items = await _startupService.GetStartupItemsAsync();
            StartupItems.Clear(); foreach (var item in items) StartupItems.Add(item);
            GeneralStatusMessage = $"{StartupItems.Count} aplicaciones de inicio detectadas.";
        }

        private async Task ChangeStartupItemAsync(object? parameter, bool enabled)
        {
            if (_startupService == null || parameter is not StartupItem item) return;
            bool success = enabled ? await _startupService.EnableStartupItemAsync(item) : await _startupService.DisableStartupItemAsync(item);
            if (success) { item.IsEnabled = enabled; OnPropertyChanged(nameof(StartupItems)); }
        }

        private async Task RefreshBloatwareAppsAsync()
        {
            if (_bloatwareService == null) return;
            var apps = await _bloatwareService.GetBloatwareAppsAsync();
            BloatwareApps.Clear(); foreach (var app in apps) BloatwareApps.Add(app);
            GeneralStatusMessage = $"{BloatwareApps.Count} aplicaciones detectadas.";
        }

        private async Task UninstallBloatwareAppAsync(object? parameter)
        {
            if (_bloatwareService == null || parameter is not BloatwareApp app) return;
            app.IsUninstalling = true;
            try { if (await _bloatwareService.UninstallBloatwareAppAsync(app)) BloatwareApps.Remove(app); }
            finally { app.IsUninstalling = false; }
        }

        private async Task RefreshUpdatableAppsAsync()
        {
            if (_wingetService == null) return;
            var apps = await _wingetService.GetUpdatableAppsAsync();
            UpdatableApps.Clear(); foreach (var app in apps) UpdatableApps.Add(app);
            GeneralStatusMessage = $"{UpdatableApps.Count} actualizaciones disponibles.";
        }

        private async Task UpdateAllAppsAsync()
        {
            if (_wingetService == null) return;
            IsBusy = true; try { await _wingetService.UpdateAllAppsAsync(); } finally { IsBusy = false; }
        }

        private async Task UpdateAppAsync(object? parameter)
        {
            if (_wingetService == null || parameter is not string id) return;
            var app = UpdatableApps.FirstOrDefault(x => x.Id == id); if (app == null) return;
            app.IsUpdating = true;
            try { await _wingetService.UpdateAppAsync(id, new Progress<(int, string)>(p => { app.UpdateProgress = p.Item1; app.UpdateStatusMessage = p.Item2; })); }
            finally { app.IsUpdating = false; }
        }

        private void ChangeAccentColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color)) return;
            if (Application.Current is App app) { app.ChangeAccentColor(color); AccentColor = color; }
            _ = SaveSettingsAsync();
        }

        private static void ApplyStartupRegistration(bool enabled)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (key == null) return;
                const string name = "WPC-SutilBox";
                if (enabled)
                {
                    string exe = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(exe)) key.SetValue(name, $"\"{exe}\"");
                }
                else key.DeleteValue(name, false);
            }
            catch { }
        }

        private void RefreshLogs()
        {
            if (_logService is not FileLogService fileLog) return;
            RecentLogEntries.Clear(); foreach (var line in fileLog.GetRecentEntries(200)) RecentLogEntries.Add(line);
        }

        private async Task RefreshProcessesAsync()
        {
            if (_processManagerService == null) return;
            var processes = await _processManagerService.GetProcessesAsync();
            Processes.Clear();
            foreach (var process in processes.Where(p => string.IsNullOrWhiteSpace(ProcessSearchText) || p.Name.Contains(ProcessSearchText, StringComparison.OrdinalIgnoreCase)))
                Processes.Add(process);
        }

        private async Task KillProcessAsync(object? parameter)
        {
            if (_processManagerService == null || parameter is not ProcessInfoDto process || process.IsCritical) return;
            if (await _processManagerService.KillProcessAsync(process.Pid)) Processes.Remove(process);
        }

        public void StartMonitoring()
        {
            if (_monitoringTask != null) return;
            _monitoringCts = new CancellationTokenSource();
            _monitoringTask = MonitorSystemAsync(_monitoringCts.Token);
        }

        private async Task MonitorSystemAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    if (now - _lastGlobalUsageSampleUtc >= GlobalUsageInterval)
                    {
                        _lastGlobalUsageSampleUtc = now;
                        await UpdateGlobalUsageAsync(cancellationToken);
                    }

                    if (now - _lastTemperatureSampleUtc >= TemperatureInterval)
                    {
                        _lastTemperatureSampleUtc = now;
                        await UpdateThermalAsync(cancellationToken);
                    }

                    if (_batteryService != null && now - _lastBatterySampleUtc >= BatteryInterval)
                    {
                        _lastBatterySampleUtc = now;
                        await UpdateBatteryAsync(cancellationToken);
                    }

                    if (OptimizeOnIdle && _monitoringService != null && RamUsage >= 85 &&
                        _monitoringService.GetIdleTime() >= TimeSpan.FromMinutes(10) &&
                        DateTime.Now - _lastIdleOptimization >= TimeSpan.FromMinutes(10))
                    {
                        _lastIdleOptimization = DateTime.Now;
                        await ExecuteOptimizeRamAsync();
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { break; }
                catch (Exception ex) { WriteLog("Error actualizando métricas del sistema", ex); }

                try { await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task ExecuteOptimizeRamAsync()
        {
            if (_processManagerService == null) return;
            IsBusy = true; StatusMessage = "Liberando memoria...";
            try
            {
                await _processManagerService.OptimizeRamAsync();
                GeneralStatusMessage = "Memoria liberada correctamente.";
                WriteLog("Limpieza de RAM completada.");
                await UpdateSystemUsageAsync();
            }
            catch (Exception ex) { GeneralStatusMessage = "No se pudo liberar la memoria."; WriteLog("Error limpiando RAM", ex); }
            finally { StatusMessage = string.Empty; IsBusy = false; }
        }

        private async Task ExecuteQuickOptimizeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Ejecutando optimización rápida...";
            try
            {
                await CleanTemporaryFilesCoreAsync();
                if (_processManagerService != null) await _processManagerService.OptimizeRamAsync();
                if (_performanceProfileService != null)
                {
                    var result = await _performanceProfileService.ApplyProfileAsync(CurrentMode);
                    GeneralStatusMessage = result.Message ?? (result.Success ? "Optimización rápida completada." : "El perfil activo no pudo aplicarse.");
                }
                else GeneralStatusMessage = "Optimización rápida completada.";
                WriteLog("Optimización rápida completada: temporales, RAM y perfil activo.");
                await UpdateSystemUsageAsync();
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = "La optimización rápida terminó con errores.";
                WriteLog("Error en la optimización rápida", ex);
            }
            finally { StatusMessage = string.Empty; IsBusy = false; }
        }

        private async Task ExecuteCreateRestorePointAsync()
        {
            if (_restorePointService == null) return;
            IsBusy = true; StatusMessage = "Creando punto de restauración...";
            try
            {
                var result = await _restorePointService.CreateRestorePointAsync($"WPC-SutilBox - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                GeneralStatusMessage = result.Message;
                StatusMessage = result.Message;
                WriteLog(result.Message);
            }
            catch (Exception ex) { GeneralStatusMessage = "No se pudo crear el punto de restauración."; WriteLog("Error creando punto de restauración", ex); }
            finally { await Task.Delay(1500); StatusMessage = string.Empty; IsBusy = false; }
        }

        private async Task ExecuteApplyPerformanceModeAsync(object? parameter)
        {
            if (_performanceProfileService == null || parameter == null) return;
            if (!Enum.TryParse<PerformanceMode>(parameter.ToString(), true, out var mode)) return;
            IsBusy = true; StatusMessage = $"Aplicando perfil {GetModeName(mode)}...";
            try
            {
                var result = await _performanceProfileService.ApplyProfileAsync(mode);
                if (result.Success) CurrentMode = mode;
                GeneralStatusMessage = result.Message ?? (result.Success ? "Perfil aplicado." : "No se pudo aplicar el perfil.");
                WriteLog($"Perfil {mode}: {GeneralStatusMessage}");
            }
            catch (Exception ex) { GeneralStatusMessage = "Error aplicando el perfil."; WriteLog($"Error aplicando perfil {mode}", ex); }
            finally { StatusMessage = string.Empty; IsBusy = false; }
        }

        private static string GetModeName(PerformanceMode mode) => mode switch
        {
            PerformanceMode.General => "Equilibrado", PerformanceMode.Gamer => "Gaming",
            PerformanceMode.Oficina => "Productividad", PerformanceMode.Dev => "Desarrollo",
            _ => "A tu medida"
        };

        // ==========================================
        // 5. MÉTODOS DE GESTIÓN DE DISCOS Y HARDWARE
        // ==========================================
        public async Task LoadDisksAsync()
        {
            Disks.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var diskInfo = GetPhysicalDiskInfoForDrive(drive);
                    if (diskInfo != null)
                    {
                        Disks.Add(diskInfo);
                    }
                }
            }
            await UpdateUnifiedDiskStatsAsync();
        }

        private DiskHealthInfo GetPhysicalDiskInfoForDrive(DriveInfo drive)
        {
            return new DiskHealthInfo
            {
                DeviceId = drive.Name,
                Model = $"Unidad local ({drive.DriveFormat})",
                Capacity = $"{drive.TotalSize / (1024 * 1024 * 1024)} GB",
                SmartStatus = "OK",
                SmartOk = true
            };
        }

        // ==========================================
        // 6. IMPLEMENTACIÓN DE COMANDOS Y ACCIONES
        // ==========================================
        private void ExecuteOpenDiscord()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://discord.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el enlace de Discord: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenCleanmgr()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cleanmgr.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo iniciar el Liberador de espacio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteCleanTempFilesAsync()
        {
            await CleanTemporaryFilesCoreAsync();
        }

        private static async Task CleanTemporaryFilesCoreAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    string tempPath = Path.GetTempPath();
                    foreach (var file in Directory.GetFiles(tempPath))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error limpiando temporales: {ex.Message}");
                }
            });
        }

        private void ExecuteOpenLogFolder()
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = logDir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir la carpeta de registros: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRunTool(object? param)
        {
            if (param is string tool && !string.IsNullOrWhiteSpace(tool))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tool,
                        UseShellExecute = true
                    });
                    WriteLog($"Herramienta ejecutada correctamente: {tool}");
                }
                catch (Exception ex)
                {
                    WriteLog($"Error al ejecutar la herramienta: {tool}", ex);
                    MessageBox.Show($"No se pudo iniciar {tool}: {ex.Message}", "Error de ejecución", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExecuteRunSfcAsync()
        {
            IsBusy = true;
            StatusMessage = "Ejecutando SFC /scannow...";
            try
            {
                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo("cmd.exe", "/c sfc /scannow")
                    {
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                });
                WriteLog("SFC /scannow iniciado.");
            }
            catch (Exception ex)
            {
                WriteLog("Error al iniciar SFC", ex);
                MessageBox.Show($"No se pudo ejecutar SFC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        private async Task ExecuteRunDismAsync()
        {
            IsBusy = true;
            StatusMessage = "Ejecutando DISM...";
            try
            {
                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo("cmd.exe", "/c DISM /Online /Cleanup-Image /RestoreHealth")
                    {
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                });
                WriteLog("DISM RestoreHealth iniciado.");
            }
            catch (Exception ex)
            {
                WriteLog("Error al iniciar DISM", ex);
                MessageBox.Show($"No se pudo ejecutar DISM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        private void ExecuteRunChkdsk()
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/k chkdsk C:")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                WriteLog("CHKDSK iniciado.");
            }
            catch (Exception ex)
            {
                WriteLog("Error al iniciar CHKDSK", ex);
                MessageBox.Show($"No se pudo ejecutar CHKDSK: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // 7. MONITOREO Y ACTUALIZACIÓN ASÍNCRONA
        // ==========================================
        public async Task UpdateSystemUsageAsync()
        {
            if (_monitoringService == null) return;
            var usage = await _monitoringService.GetSystemUsageAsync();
            CpuUsage = usage.CpuUsage;
            RamUsage = usage.RamUsage;
            NetRecvMbps = usage.NetBytesReceivedPerSec / (1024d * 1024d);
            DiskWritesPerSec = usage.DiskWritesPerSec;
            ActiveTcpConnections = usage.ActiveTcpConnections;
            CpuPerCore.Clear();
            for (int i = 0; i < usage.CpuPerCore.Length; i++) CpuPerCore.Add(new CpuCoreMetric { Index = i, Usage = usage.CpuPerCore[i] });
        }

        private async Task UpdateGlobalUsageAsync(CancellationToken cancellationToken)
        {
            if (_monitoringService == null) return;

            var usage = await _monitoringService.GetGlobalUsageAsync(cancellationToken);
            if (usage.CpuUsage.HasValue) CpuUsage = usage.CpuUsage.Value;
            if (usage.RamUsage.HasValue) RamUsage = usage.RamUsage.Value;
        }

        private async Task UpdateBatteryAsync(CancellationToken cancellationToken)
        {
            if (_batteryService == null) return;

            cancellationToken.ThrowIfCancellationRequested();
            var battery = await _batteryService.GetBatteryStatusAsync();
            cancellationToken.ThrowIfCancellationRequested();

            if (battery.IsReadSuccessful)
            {
                BatteryInfo = battery;
            }
        }

        public async Task UpdateThermalAsync(CancellationToken cancellationToken = default)
        {
            if (_temperatureMonitorService == null) return;

            cancellationToken.ThrowIfCancellationRequested();
            var temp = await _temperatureMonitorService.GetCpuTemperatureCAsync();
            cancellationToken.ThrowIfCancellationRequested();

            if (temp.HasValue)
            {
                CpuTempC = temp.Value;
                CpuTemperatureStatus = "Correcto";
            }
            else if (CpuTempC.HasValue)
            {
                CpuTemperatureStatus = "Lectura fallida; último valor válido";
            }
            else
            {
                CpuTemperatureStatus = "No disponible";
            }
        }

        public async Task UpdateUnifiedDiskStatsAsync()
        {
            if (_diskHealthService != null)
            {
                var diskInfo = await _diskHealthService.GetDiskHealthAsync();
            }
        }

        // ==========================================
        // 8. CICLO DE VIDA DE LA VENTANA
        // ==========================================
        public void MinimizeToTrayWindow()
        {
            IsWindowVisible = false;
        }

        public async Task PrepareForShutdownAsync()
        {
            if (_monitoringCts != null)
            {
                _monitoringCts.Cancel();
                if (_monitoringTask != null)
                {
                    try { await _monitoringTask; }
                    catch (OperationCanceledException) { }
                }

                _monitoringCts.Dispose();
                _monitoringCts = null;
                _monitoringTask = null;
            }
        }

        // ==========================================
        // 9. UTILIDAD DE REGISTRO (LOGS)
        // ==========================================
        public static void WriteLog(string message, Exception? ex = null)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string logFilePath = Path.Combine(logDir, "app.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                if (ex != null)
                {
                    logEntry += $" | EXCEPCIÓN: {ex.GetType().Name} - Mensaje: {ex.Message} | UBICACIÓN/STACK: {ex.StackTrace}";
                }

                logEntry += Environment.NewLine;
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception writeEx)
            {
                Debug.WriteLine($"No se pudo escribir el archivo de log: {writeEx.Message}");
            }
        }
    }
}
