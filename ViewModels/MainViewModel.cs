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
        private readonly ISystemMaintenanceService? _systemMaintenanceService;
        private readonly IDialogService? _dialogService;
        private IReadOnlyList<CleanupCandidate> _cleanupCandidates = Array.Empty<CleanupCandidate>();
        private bool _cleanSystemTemp = true;
        private bool _cleanRecycleBin;
        private bool _cleanBrowserCache;
        private readonly IMonitoringService? _monitoringService;
        private readonly IBatteryService? _batteryService;
        private readonly IProcessManagerService? _processManagerService;
        private readonly IRestorePointService? _restorePointService;
        private readonly IPerformanceProfileService? _performanceProfileService;
        private readonly IStartupService? _startupService;
        private readonly IBloatwareService? _bloatwareService;
        private readonly IWingetService? _wingetService;
        private readonly IServiceOptimizerService? _serviceOptimizerService;
        private readonly ISettingsService? _settingsService;
        private readonly ILogService? _logService;
        private readonly ILocalizationService? _localizationService;
        private readonly IDriverService? _driverService;
        private readonly IBackupService? _backupService;
        private readonly IRestoreService? _restoreService;
        private readonly ProfileEditorViewModel? _profileEditorViewModel;
        private CancellationTokenSource? _monitoringCts;
        private CancellationTokenSource? _driverExportCts;
        private CancellationTokenSource? _backupCts;
        private CancellationTokenSource? _restoreCts;
        private Task? _monitoringTask;
        private int _navigationSequence;
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
        private AppSection _currentSection = AppSection.Dashboard;
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
        private string _serviceSearchText = string.Empty;
        private string _serviceStatusMessage = "Lista de servicios sin cargar.";
        private readonly List<WindowsService> _allWindowsServices = new();
        private double _netRecvMbps;
        private double _diskWritesPerSec;
        private bool _isExportingDrivers;
        private int _driverExportProgress;
        private string _driverExportStatusMessage = string.Empty;

        // Backup properties
        private string _backupDestinationPath = string.Empty;
        private bool _backupDrivers = true;
        private bool _backupWifi = true;
        private bool _backupSoftware = true;
        private bool _backupUserSettings = true;
        private bool _isBackupRunning;
        private int _backupProgress;
        private string _backupStatusMessage = string.Empty;
        private string _backupProgressText = string.Empty;
        private bool _backupCompleted;
        private string _backupResultMessage = string.Empty;

        // Restore properties
        private string _selectedBackupPath = string.Empty;
        private bool _hasSelectedBackup;
        private DateTime _backupInfo = DateTime.MinValue;
        private int _softwareCount;
        private int _wifiCount;
        private long _backupSize;
        private bool _restoreDrivers = true;
        private bool _restoreWifi = true;
        private bool _restoreSoftware = true;
        private bool _restoreUserSettings = true;
        private bool _isRestoreRunning;
        private int _restoreProgress;
        private string _restoreStatusMessage = string.Empty;
        private string _restoreProgressText = string.Empty;
        private bool _restoreCompleted;
        private string _restoreResultMessage = string.Empty;

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
        public string ServiceSearchText
        {
            get => _serviceSearchText;
            set
            {
                if (SetProperty(ref _serviceSearchText, value))
                    ApplyServiceFilter();
            }
        }
        public string ServiceStatusMessage { get => _serviceStatusMessage; private set => SetProperty(ref _serviceStatusMessage, value); }
        public bool RunOnStartup { get => _runOnStartup; set => SetSetting(ref _runOnStartup, value); }
        public bool OptimizeOnIdle { get => _optimizeOnIdle; set => SetSetting(ref _optimizeOnIdle, value); }
        public bool MinimizeToTray { get => _minimizeToTray; set => SetSetting(ref _minimizeToTray, value); }
        public bool IsDarkMode { get => _isDarkMode; set { if (SetProperty(ref _isDarkMode, value) && _settingsReady) { if (Application.Current is App app) app.ChangeTheme(value); _ = SaveSettingsAsync(); } } }
        public string SelectedLanguage { get => _selectedLanguage; set => SetSetting(ref _selectedLanguage, value); }
        public string AccentColor { get => _accentColor; private set => SetProperty(ref _accentColor, value); }
        public double NetRecvMbps { get => _netRecvMbps; private set => SetProperty(ref _netRecvMbps, value); }
        public double DiskWritesPerSec { get => _diskWritesPerSec; private set => SetProperty(ref _diskWritesPerSec, value); }
        public bool IsExportingDrivers { get => _isExportingDrivers; private set => SetProperty(ref _isExportingDrivers, value); }
        public int DriverExportProgress { get => _driverExportProgress; private set => SetProperty(ref _driverExportProgress, value); }
        public string DriverExportStatusMessage { get => _driverExportStatusMessage; private set => SetProperty(ref _driverExportStatusMessage, value); }
        public ProfileEditorViewModel? ProfileEditorViewModel => _profileEditorViewModel;

        // Backup properties
        public string BackupDestinationPath { get => _backupDestinationPath; set => SetProperty(ref _backupDestinationPath, value); }
        public bool BackupDrivers { get => _backupDrivers; set => SetProperty(ref _backupDrivers, value); }
        public bool BackupWifi { get => _backupWifi; set => SetProperty(ref _backupWifi, value); }
        public bool BackupSoftware { get => _backupSoftware; set => SetProperty(ref _backupSoftware, value); }
        public bool BackupUserSettings { get => _backupUserSettings; set => SetProperty(ref _backupUserSettings, value); }
        public bool IsBackupRunning { get => _isBackupRunning; private set => SetProperty(ref _isBackupRunning, value); }
        public int BackupProgress { get => _backupProgress; private set => SetProperty(ref _backupProgress, value); }
        public string BackupStatusMessage { get => _backupStatusMessage; private set => SetProperty(ref _backupStatusMessage, value); }
        public string BackupProgressText { get => _backupProgressText; private set => SetProperty(ref _backupProgressText, value); }
        public bool BackupCompleted { get => _backupCompleted; private set => SetProperty(ref _backupCompleted, value); }
        public string BackupResultMessage { get => _backupResultMessage; private set => SetProperty(ref _backupResultMessage, value); }

        // Restore properties
        public string SelectedBackupPath { get => _selectedBackupPath; private set { SetProperty(ref _selectedBackupPath, value); HasSelectedBackup = !string.IsNullOrEmpty(value); } }
        public bool HasSelectedBackup { get => _hasSelectedBackup; private set => SetProperty(ref _hasSelectedBackup, value); }
        public DateTime BackupInfo { get => _backupInfo; private set => SetProperty(ref _backupInfo, value); }
        public int SoftwareCount { get => _softwareCount; private set => SetProperty(ref _softwareCount, value); }
        public int WifiCount { get => _wifiCount; private set => SetProperty(ref _wifiCount, value); }
        public long BackupSize { get => _backupSize; private set => SetProperty(ref _backupSize, value); }
        public bool RestoreDrivers { get => _restoreDrivers; set => SetProperty(ref _restoreDrivers, value); }
        public bool RestoreWifi { get => _restoreWifi; set => SetProperty(ref _restoreWifi, value); }
        public bool RestoreSoftware { get => _restoreSoftware; set => SetProperty(ref _restoreSoftware, value); }
        public bool RestoreUserSettings { get => _restoreUserSettings; set => SetProperty(ref _restoreUserSettings, value); }
        public bool IsRestoreRunning { get => _isRestoreRunning; private set => SetProperty(ref _isRestoreRunning, value); }
        public int RestoreProgress { get => _restoreProgress; private set => SetProperty(ref _restoreProgress, value); }
        public string RestoreStatusMessage { get => _restoreStatusMessage; private set => SetProperty(ref _restoreStatusMessage, value); }
        public string RestoreProgressText { get => _restoreProgressText; private set => SetProperty(ref _restoreProgressText, value); }
        public bool RestoreCompleted { get => _restoreCompleted; private set => SetProperty(ref _restoreCompleted, value); }
        public string RestoreResultMessage { get => _restoreResultMessage; private set => SetProperty(ref _restoreResultMessage, value); }

        public ObservableCollection<BackupInfo> ExistingBackups { get; } = new();

        public ObservableCollection<StartupItem> StartupItems { get; } = new();
        public ObservableCollection<BloatwareApp> BloatwareApps { get; } = new();
        public ObservableCollection<WingetApp> UpdatableApps { get; } = new();
        public ObservableCollection<string> RecentLogEntries { get; } = new();
        public ObservableCollection<CpuCoreMetric> CpuPerCore { get; } = new();
        public bool CleanSystemTemp { get => _cleanSystemTemp; set => SetProperty(ref _cleanSystemTemp, value); }
        public bool CleanRecycleBin { get => _cleanRecycleBin; set => SetProperty(ref _cleanRecycleBin, value); }
        public bool CleanBrowserCache { get => _cleanBrowserCache; set => SetProperty(ref _cleanBrowserCache, value); }
        public IReadOnlyList<CleanupCandidate> CleanupCandidates
        {
            get => _cleanupCandidates;
            private set => SetProperty(ref _cleanupCandidates, value);
        }
        public int CleanupCandidateCount => CleanupCandidates.Count;
        public long CleanupEstimatedBytes => CleanupCandidates.Sum(candidate => candidate.SizeBytes);
        public ObservableCollection<ProcessInfoDto> Processes { get; } = new();
        public ObservableCollection<WindowsService> WindowsServices { get; } = new();
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

        public string ApplicationVersion => typeof(MainViewModel).Assembly.GetName().Version?.ToString(3) ?? "desconocida";

        public string ApplicationVersionLabel => $"Versión {ApplicationVersion}";

        public string AboutDescription =>
            $"Versión {ApplicationVersion} — Suite integral de mantenimiento y diagnóstico de Windows.";

        public AppSection CurrentSection
        {
            get => _currentSection;
            set
            {
                if (SetProperty(ref _currentSection, value))
                {
                    OnPropertyChanged(nameof(CurrentSectionTitle));
                    _logService?.Info($"[Navegación] CurrentSection cambió a: {value}");
                    WriteLog($"[Navegación] Sección activa: {value}");
                }
            }
        }

        public string CurrentSectionTitle => CurrentSection switch
        {
            AppSection.Dashboard => "Panel principal",
            AppSection.RevisarPC => "Mi PC",
            AppSection.EditorPerfiles => "Editor de perfiles",
            AppSection.Batería => "Batería",
            AppSection.Benchmark => "Benchmark",
            AppSection.Drivers => "Drivers",
            AppSection.SaludDiscos => "Salud de discos",
            AppSection.VerificarUSB => "Verificar USB",
            AppSection.Periféricos => "Periféricos",
            AppSection.TestRed => "Test de red",
            AppSection.RepararWindows => "Reparar Windows",
            AppSection.CompatibilidadWin11 => "Compatibilidad Windows 11",
            AppSection.ArchivosDuplicados => "Archivos duplicados",
            AppSection.Limpieza => "Limpieza",
            AppSection.MapaEspacio => "Mapa del disco",
            AppSection.Almacenamiento => "Almacenamiento",
            AppSection.Desinstalador => "Aplicaciones instaladas",
            AppSection.Bloatware => "Bloatware",
            AppSection.ProgramasInstalar or AppSection.ProgramasActualizar => "Programas y actualizaciones",
            AppSection.InicioWindows => "Inicio de Windows",
            AppSection.Optimizaciones => "Optimizaciones",
            AppSection.Privacidad => "Privacidad",
            AppSection.Rendimiento => "Rendimiento",
            AppSection.CentralDeshacer => "Central Deshacer",
            AppSection.PuntosRestauracion => "Puntos de restauración",
            AppSection.BackupFormatear => "Backup para formatear",
            AppSection.PostFormateo => "Post-formateo",
            AppSection.Red => "Red",
            AppSection.Herramientas => "Herramientas",
            AppSection.Historial => "Historial",
            AppSection.Configuracion => "Configuración",
            AppSection.Hardware => "Hardware",
            AppSection.AcercaDe => "Acerca de",
            AppSection.Escala => "Tamaño de letra",
            _ => "Centro de control"
        };

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
        public ICommand PreviewCleanupCommand { get; }
        public ICommand OpenSpaceMapCommand { get; }
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
        public ICommand ExportDriversCommand { get; }
        public ICommand CancelDriverExportCommand { get; }

        // Comandos de mantenimiento y optimización
        public ICommand FlushDnsCommand { get; }
        public ICommand CleanPrefetchCommand { get; }
        public ICommand RebuildSearchIndexCommand { get; }
        public ICommand ResetNetworkCommand { get; }
        public ICommand AnalyzeDiskCommand { get; }
        public ICommand RefreshDisksCommand { get; }
        public ICommand AnalyzeCommand { get; }
        public ICommand OptimizeCommand { get; }
        public ICommand RefreshDriversCommand { get; }
        public ICommand ReduceBackgroundProcessesCommand { get; }
        public ICommand ClearServiceSearchCommand { get; }
        public ICommand RefreshServicesCommand { get; }
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }
        public ICommand RefreshThermalCommand { get; }

        // Backup & Restore commands
        public ICommand? SelectBackupDestinationCommand { get; set; }
        public ICommand? StartBackupCommand { get; set; }
        public ICommand? CancelBackupCommand { get; set; }
        public ICommand? RefreshBackupsCommand { get; set; }
        public ICommand? OpenBackupFolderCommand { get; set; }
        public ICommand? DeleteBackupCommand { get; set; }
        public ICommand? SelectBackupCommand { get; set; }
        public ICommand? SelectBackupByPathCommand { get; set; }
        public ICommand? StartRestoreCommand { get; set; }
        public ICommand? CancelRestoreCommand { get; set; }

        // ==========================================
        // ViewModels dedicados (Beta 1.2)
        // ==========================================
        public StorageViewModel? StorageViewModel { get; }
        public DashboardViewModel? DashboardViewModel { get; }
        public PcReviewViewModel? PcReviewViewModel { get; }
        public BatteryViewModel? BatteryViewModel { get; }
        public DriversViewModel? DriversViewModel { get; }
        public CleanupViewModel? CleanupViewModel { get; }
        public StartupViewModel? StartupViewModel { get; }
        public ProgramsViewModel? ProgramsViewModel { get; }
        public RestorePointViewModel? RestorePointViewModel { get; }
        public PrivacyViewModel? PrivacyViewModel { get; }
        public HistoryViewModel? HistoryViewModel { get; }
        public SpaceMapViewModel? SpaceMapViewModel { get; }
        public NetworkTestViewModel? NetworkTestViewModel { get; }
        public SmartScannerViewModel? SmartScannerViewModel { get; }
        public Windows11CompatibilityViewModel? Windows11CompatibilityViewModel { get; }
        public DuplicateFilesViewModel? DuplicateFilesViewModel { get; }
        public PeripheralDiagnosticsViewModel? PeripheralDiagnosticsViewModel { get; }
        public UsbStorageViewModel? UsbStorageViewModel { get; }
        public BenchmarkViewModel? BenchmarkViewModel { get; }
        public GamesViewModel? GamesViewModel { get; set; }

        // ==========================================
        // 4. CONSTRUCTOR PRINCIPAL
        // ==========================================
        public MainViewModel(
            ITemperatureMonitorService? temperatureMonitorService = null,
            IDiskHealthService? diskHealthService = null,
            ISystemMaintenanceService? systemMaintenanceService = null,
            IDialogService? dialogService = null,
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
            ILocalizationService? localizationService = null,
            IDriverService? driverService = null,
            IBackupService? backupService = null,
            IRestoreService? restoreService = null,
            StorageViewModel? storageViewModel = null,
            DashboardViewModel? dashboardViewModel = null,
            PcReviewViewModel? pcReviewViewModel = null,
            BatteryViewModel? batteryViewModel = null,
            DriversViewModel? driversViewModel = null,
            CleanupViewModel? cleanupViewModel = null,
            StartupViewModel? startupViewModel = null,
            ProgramsViewModel? programsViewModel = null,
            RestorePointViewModel? restorePointViewModel = null,
            PrivacyViewModel? privacyViewModel = null,
            HistoryViewModel? historyViewModel = null,
            SpaceMapViewModel? spaceMapViewModel = null,
            NetworkTestViewModel? networkTestViewModel = null,
            IServiceOptimizerService? serviceOptimizerService = null,
            Windows11CompatibilityViewModel? windows11CompatibilityViewModel = null,
            DuplicateFilesViewModel? duplicateFilesViewModel = null,
            PeripheralDiagnosticsViewModel? peripheralDiagnosticsViewModel = null,
            UsbStorageViewModel? usbStorageViewModel = null,
            BenchmarkViewModel? benchmarkViewModel = null,
            SmartScannerViewModel? smartScannerViewModel = null)
        {
            StorageViewModel = storageViewModel;
            DashboardViewModel = dashboardViewModel;
            PcReviewViewModel = pcReviewViewModel;
            BatteryViewModel = batteryViewModel;
            DriversViewModel = driversViewModel;
            CleanupViewModel = cleanupViewModel;
            StartupViewModel = startupViewModel;
            ProgramsViewModel = programsViewModel;
            RestorePointViewModel = restorePointViewModel;
            PrivacyViewModel = privacyViewModel;
            HistoryViewModel = historyViewModel;
            SpaceMapViewModel = spaceMapViewModel;
            NetworkTestViewModel = networkTestViewModel;
            Windows11CompatibilityViewModel = windows11CompatibilityViewModel;
            DuplicateFilesViewModel = duplicateFilesViewModel;
            PeripheralDiagnosticsViewModel = peripheralDiagnosticsViewModel;
            UsbStorageViewModel = usbStorageViewModel;
            BenchmarkViewModel = benchmarkViewModel;
            if (SpaceMapViewModel != null)
                SpaceMapViewModel.BackCommand = new RelayCommand(_ => StartNavigation(AppSection.Limpieza.ToString()));
            _temperatureMonitorService = temperatureMonitorService;
            _diskHealthService = diskHealthService;
            _systemMaintenanceService = systemMaintenanceService;
            _dialogService = dialogService;
            _monitoringService = monitoringService;
            _batteryService = batteryService;
            _processManagerService = processManagerService;
            _restorePointService = restorePointService;
            _performanceProfileService = performanceProfileService;
            _startupService = startupService;
            _bloatwareService = bloatwareService;
            _wingetService = wingetService;
            _serviceOptimizerService = serviceOptimizerService;
            _settingsService = settingsService;
            _logService = logService;
            _profileEditorViewModel = profileEditorViewModel;
            _localizationService = localizationService;
            _driverService = driverService;
            _backupService = backupService;
            _restoreService = restoreService;

            // Set default backup path
            BackupDestinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "WPC-SutilBox Backups", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            ApplicationTitle = "WPC-SutilBox";
            SystemStatus = "WPC-SutilBox - Listo";

            CurrentView = this;
            OpenDiscordCommand = new RelayCommand(ExecuteOpenDiscord);
            OpenCleanmgrCommand = new RelayCommand(ExecuteOpenCleanmgr);
            CleanTempFilesCommand = new AsyncRelayCommand(PreviewCleanupAsync);
            PreviewCleanupCommand = new AsyncRelayCommand(PreviewCleanupAsync);
            OpenSpaceMapCommand = new RelayCommand(_ => StartNavigation(AppSection.MapaEspacio.ToString()));
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
            ExportDriversCommand = new AsyncRelayCommand(ExportDriversAsync);
            CancelDriverExportCommand = new RelayCommand(_ => _driverExportCts?.Cancel());

            // Comandos de mantenimiento y optimización
            FlushDnsCommand = new RelayCommand(ExecuteFlushDns);
            CleanPrefetchCommand = new RelayCommand(ExecuteCleanPrefetch);
            RebuildSearchIndexCommand = new RelayCommand(ExecuteRebuildSearchIndex);
            ResetNetworkCommand = new RelayCommand(ExecuteResetNetwork);
            AnalyzeDiskCommand = new RelayCommand(ExecuteAnalyzeDisk);
            RefreshDisksCommand = new AsyncRelayCommand(_ => LoadDisksAsync());
            AnalyzeCommand = new RelayCommand(ExecuteAnalyze);
            OptimizeCommand = new AsyncRelayCommand(_ => ExecuteQuickOptimizeAsync());
            RefreshDriversCommand = new AsyncRelayCommand(ExportDriversAsync);
            ReduceBackgroundProcessesCommand = new RelayCommand(ExecuteReduceBackgroundProcesses);
            ClearServiceSearchCommand = new RelayCommand(_ => ServiceSearchText = string.Empty);
            RefreshServicesCommand = new AsyncRelayCommand(_ => LoadWindowsServicesAsync());
            StartServiceCommand = new AsyncRelayCommand(p => ChangeWindowsServiceAsync(p, start: true));
            StopServiceCommand = new AsyncRelayCommand(p => ChangeWindowsServiceAsync(p, start: false));
            RefreshThermalCommand = new AsyncRelayCommand(async () => { var ct = default(CancellationToken); await UpdateThermalAsync(ct); });

            // Backup & Restore commands
            SelectBackupDestinationCommand = new RelayCommand(SelectBackupDestination);
            StartBackupCommand = new AsyncRelayCommand(StartBackupAsync);
            CancelBackupCommand = new RelayCommand(_ => _backupCts?.Cancel());
            RefreshBackupsCommand = new AsyncRelayCommand(RefreshBackupsAsync);
            OpenBackupFolderCommand = new RelayCommand(p => OpenBackupFolder(p as string));
            DeleteBackupCommand = new AsyncRelayCommand(DeleteBackupAsync);
            SelectBackupCommand = new RelayCommand(SelectBackupFolder);
            SelectBackupByPathCommand = new RelayCommand(p => SelectBackupByPath(p as string));
            StartRestoreCommand = new AsyncRelayCommand(StartRestoreAsync);
            CancelRestoreCommand = new RelayCommand(_ => _restoreCts?.Cancel());

            _ = LoadSettingsAsync();

            NavigateCommand = new RelayCommand(StartNavigation);

            if (DashboardViewModel != null)
                _ = DashboardViewModel.OnNavigatedToAsync();

            SmartScannerViewModel = smartScannerViewModel;
            WriteLog("Aplicación iniciada correctamente.");
        }

        private async Task LoadWindowsServicesAsync()
        {
            if (_serviceOptimizerService == null)
            {
                ServiceStatusMessage = "El servicio de administración no está disponible.";
                return;
            }

            ServiceStatusMessage = "Cargando servicios de Windows...";
            try
            {
                var services = await _serviceOptimizerService.GetWindowsServicesAsync();
                _allWindowsServices.Clear();
                _allWindowsServices.AddRange(services);
                ApplyServiceFilter();
            }
            catch (Exception ex)
            {
                ServiceStatusMessage = $"Error al cargar servicios: {ex.Message}";
                WriteLog("Error al cargar servicios de Windows", ex);
            }
        }

        private void ApplyServiceFilter()
        {
            string search = ServiceSearchText.Trim();
            var filtered = _allWindowsServices.Where(service =>
                string.IsNullOrEmpty(search)
                || service.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || service.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || service.Description.Contains(search, StringComparison.OrdinalIgnoreCase));

            WindowsServices.Clear();
            foreach (var service in filtered)
                WindowsServices.Add(service);

            ServiceStatusMessage = string.IsNullOrEmpty(search)
                ? $"{WindowsServices.Count} servicios encontrados."
                : $"{WindowsServices.Count} servicios coinciden con la búsqueda.";
        }

        private async Task ChangeWindowsServiceAsync(object? parameter, bool start)
        {
            if (parameter is not string serviceName || string.IsNullOrWhiteSpace(serviceName))
                return;

            if (_serviceOptimizerService == null || _dialogService == null)
            {
                ServiceStatusMessage = "No se puede confirmar o ejecutar la acción en este momento.";
                return;
            }

            var service = _allWindowsServices.FirstOrDefault(item =>
                string.Equals(item.Name, serviceName, StringComparison.OrdinalIgnoreCase));
            if (service == null || (start && !service.CanBeStarted) || (!start && !service.CanBeStopped))
            {
                ServiceStatusMessage = "El servicio seleccionado no admite esta acción.";
                return;
            }

            string action = start ? "iniciar" : "detener";
            string actionPast = start ? "iniciado" : "detenido";
            try
            {
                bool confirmed = await _dialogService.ShowConfirmation(
                    $"¿Desea {action} el servicio \"{service.DisplayName}\"? Esto puede afectar a Windows o a aplicaciones que dependan de él.",
                    "Confirmar acción sobre servicio");
                if (!confirmed)
                {
                    ServiceStatusMessage = "Acción cancelada; el servicio no se modificó.";
                    return;
                }

                bool succeeded = start
                    ? await _serviceOptimizerService.StartServiceAsync(service.Name)
                    : await _serviceOptimizerService.StopServiceAsync(service.Name);

                if (!succeeded)
                {
                    ServiceStatusMessage = $"No se pudo {action} el servicio. Compruebe los permisos y su estado.";
                    return;
                }

                ServiceStatusMessage = $"Servicio {actionPast} correctamente.";
                await LoadWindowsServicesAsync();
            }
            catch (Exception ex)
            {
                ServiceStatusMessage = $"Error al {action} el servicio: {ex.Message}";
                WriteLog($"Error al {action} el servicio {service.Name}", ex);
            }
        }

        private void StartNavigation(object? parameter)
        {
            var sequence = Interlocked.Increment(ref _navigationSequence);
            _ = NavigateAsync(parameter, sequence);
        }

        private async Task NavigateAsync(object? parameter, int sequence)
        {
            if (parameter is not string section || string.IsNullOrWhiteSpace(section))
                return;

            if (!Enum.TryParse(section, ignoreCase: true, out AppSection targetSection))
            {
                WriteLog($"Sección desconocida: {section}");
                return;
            }

            targetSection = AppSectionRoutes.Canonicalize(targetSection);

            try
            {
                if (CurrentSection != targetSection)
                    await OnNavigatedFromAsync(CurrentSection);

                if (sequence != Volatile.Read(ref _navigationSequence))
                    return;

                CurrentSection = targetSection;
                WriteLog($"Navegando a la sección: {targetSection}");
                await OnNavigatedToAsync(targetSection);

                if (sequence != Volatile.Read(ref _navigationSequence) && CurrentSection != targetSection)
                    await OnNavigatedFromAsync(targetSection);
            }
            catch (Exception ex)
            {
                WriteLog($"Error al navegar a la sección {targetSection}", ex);
            }
        }

        private async Task OnNavigatedToAsync(AppSection section)
        {
            switch (section)
            {
                case AppSection.SaludDiscos when StorageViewModel != null:
                    await StorageViewModel.RefreshDiskHealthAsync();
                    break;
                case AppSection.Dashboard when DashboardViewModel != null:
                    await DashboardViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.ZonaGamer when GamesViewModel != null:
                    await GamesViewModel.RefreshAsync();
                    break;
                case AppSection.RevisarPC when PcReviewViewModel != null:
                    await PcReviewViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Batería when BatteryViewModel != null:
                    await BatteryViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Drivers when DriversViewModel != null:
                    await DriversViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Desinstalador when ProgramsViewModel != null:
                    await ProgramsViewModel.LoadInstalledAppsAsync();
                    break;
                case AppSection.ProgramasActualizar when ProgramsViewModel != null:
                    await ProgramsViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.InicioWindows when StartupViewModel != null:
                    await StartupViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Privacidad when PrivacyViewModel != null:
                    await PrivacyViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Rendimiento:
                    await LoadWindowsServicesAsync();
                    break;
                case AppSection.CompatibilidadWin11 when Windows11CompatibilityViewModel != null:
                    await Windows11CompatibilityViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Periféricos when PeripheralDiagnosticsViewModel != null:
                    await PeripheralDiagnosticsViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.VerificarUSB when UsbStorageViewModel != null:
                    await UsbStorageViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.CentralDeshacer when HistoryViewModel != null:
                    await HistoryViewModel.OnNavigatedToAsync();
                    break;
                case AppSection.Historial:
                    RefreshLogs();
                    break;
            }
        }

        private async Task OnNavigatedFromAsync(AppSection section)
        {
            switch (section)
            {
                case AppSection.Dashboard when DashboardViewModel != null:
                    await DashboardViewModel.OnNavigatedFromAsync();
                    break;
                case AppSection.RevisarPC when PcReviewViewModel != null:
                    await PcReviewViewModel.OnNavigatedFromAsync();
                    break;
            }
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
            if (UpdatableApps.Count == 0)
            {
                MessageBox.Show("No hay actualizaciones pendientes para instalar.", "WPC-SutilBox", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Se van a actualizar {UpdatableApps.Count} aplicaciones mediante Winget en segundo plano.\n\n¿Deseas continuar con la actualización?",
                "Confirmar Actualización Global",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsBusy = true;
            try
            {
                GeneralStatusMessage = "Actualizando todas las aplicaciones...";
                await _wingetService.UpdateAllAppsAsync();
                await RefreshUpdatableAppsAsync();
            }
            finally
            {
                IsBusy = false;
            }
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
            if (_dialogService == null) return;
            var confirmed = await _dialogService.ShowConfirmation(
                $"Se terminará el proceso '{process.Name}' (PID {process.Pid}). Puede perder datos sin guardar y no se puede deshacer. ¿Deseas continuar?",
                "Confirmar cierre de proceso");
            if (!confirmed) return;
            if (await _processManagerService.KillProcessAsync(process.Pid))
            {
                Processes.Remove(process);
                WriteLog($"Proceso terminado tras confirmación: {process.Name} (PID {process.Pid}).");
            }
            else
            {
                GeneralStatusMessage = "Windows rechazó el cierre del proceso o está protegido.";
            }
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
            await PreviewCleanupAsync();
        }

        private async Task PreviewCleanupAsync()
        {
            if (IsBusy) return;
            if (!CleanSystemTemp)
            {
                GeneralStatusMessage = "Activa Archivos Temporales para analizar la limpieza.";
                return;
            }
            if (_systemMaintenanceService == null || _dialogService == null)
            {
                GeneralStatusMessage = "La limpieza controlada no está disponible.";
                return;
            }

            IsBusy = true;
            StatusMessage = "Analizando archivos temporales...";
            try
            {
                var options = new CleaningOptions
                {
                    CleanSystemTemp = CleanSystemTemp,
                    CleanRecycleBin = false,
                    CleanBrowserCache = false,
                    CleanWindowsUpdate = false,
                    CleanThumbnails = false,
                    CleanEventLogs = false
                };
                CleanupCandidates = await _systemMaintenanceService.PreviewTemporaryFilesAsync(options);
                OnPropertyChanged(nameof(CleanupCandidateCount));
                OnPropertyChanged(nameof(CleanupEstimatedBytes));

                if (CleanupCandidates.Count == 0)
                {
                    GeneralStatusMessage = "No se encontraron archivos temporales compatibles para limpiar.";
                    WriteLog("Previsualización de limpieza: no se encontraron candidatos.");
                    return;
                }

                var candidateList = string.Join(
                    Environment.NewLine,
                    CleanupCandidates.Select(candidate => $"• {candidate.Path} ({FormatBytes(candidate.SizeBytes)})"));
                var confirmation = await _dialogService.ShowConfirmation(
                    $"Se encontraron {CleanupCandidateCount} archivos temporales (tamaño total estimado: {FormatBytes(CleanupEstimatedBytes)}).\n\nCandidatos que se intentarán eliminar:\n{candidateList}\n\nSolo se eliminarán estos archivos dentro de las carpetas temporales autorizadas. No se vaciará la Papelera ni se limpiarán cachés de navegador o Windows Update.\n\n¿Deseas continuar?",
                    "Revisar limpieza temporal");
                if (!confirmation)
                {
                    GeneralStatusMessage = "Limpieza cancelada; no se eliminaron archivos.";
                    WriteLog("El usuario canceló la limpieza tras revisar la previsualización.");
                    return;
                }

                StatusMessage = "Limpiando archivos temporales...";
                var result = await _systemMaintenanceService.CleanTemporaryFilesAsync(CleanupCandidates);
                GeneralStatusMessage = result.ToString();
                WriteLog($"Limpieza controlada completada. {result}");
                CleanupCandidates = Array.Empty<CleanupCandidate>();
                OnPropertyChanged(nameof(CleanupCandidateCount));
                OnPropertyChanged(nameof(CleanupEstimatedBytes));
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = "No se pudo completar el análisis o limpieza temporal.";
                WriteLog("Error en limpieza controlada", ex);
            }
            finally
            {
                StatusMessage = string.Empty;
                IsBusy = false;
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < sizes.Length - 1) { value /= 1024; unit++; }
            return $"{value:0.##} {sizes[unit]}";
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

        private async Task ExportDriversAsync()
        {
            if (_driverService == null || IsExportingDrivers) return;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var destinationPath = Path.Combine(desktopPath, "WPC-SutilBox Backups", "Drivers", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            var confirmation = MessageBox.Show(
                $"Se exportarán los controladores de terceros mediante DISM.\n\nDestino:\n{destinationPath}\n\n¿Deseas continuar?",
                "Confirmar copia de seguridad de controladores",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes) return;

            _driverExportCts = new CancellationTokenSource();
            IsExportingDrivers = true;
            DriverExportProgress = 0;
            DriverExportStatusMessage = "Preparando exportación de controladores...";

            try
            {
                var progress = new Progress<(int Percentage, string Message)>(update =>
                {
                    DriverExportProgress = update.Percentage;
                    DriverExportStatusMessage = update.Message;
                });

                var result = await _driverService.ExportDriversAsync(destinationPath, progress, _driverExportCts.Token);
                DriverExportStatusMessage = result.Success
                    ? $"Copia creada en: {destinationPath}"
                    : result.Message;
                GeneralStatusMessage = result.Success
                    ? "Copia de seguridad de controladores completada."
                    : result.Message;
                WriteLog($"Backup de drivers: {GeneralStatusMessage}");
            }
            catch (Exception ex)
            {
                DriverExportStatusMessage = "No se pudo exportar la copia de seguridad.";
                GeneralStatusMessage = DriverExportStatusMessage;
                WriteLog("Error exportando backup de drivers", ex);
            }
            finally
            {
                _driverExportCts.Dispose();
                _driverExportCts = null;
                IsExportingDrivers = false;
            }
        }

        private async Task ExecuteApplyPerformanceModeAsync(object? parameter)
        {
            if (_performanceProfileService == null || _dialogService == null || parameter == null) return;
            if (!Enum.TryParse<PerformanceMode>(parameter.ToString(), true, out var mode) || !Enum.IsDefined(mode)) return;
            var performanceProfileService = _performanceProfileService;
            var dialogService = _dialogService;
            var confirmed = await dialogService.ShowConfirmation(
                mode == PerformanceMode.General
                    ? "Se restaurarán los valores guardados en la captura del estado original. ¿Desea continuar?"
                    : $"El perfil {GetModeName(mode)} puede cambiar el plan de energía y los servicios configurados. Se intentará crear un punto de restauración antes de aplicar los cambios. ¿Desea continuar?",
                $"Confirmar perfil {GetModeName(mode)}");
            if (!confirmed) return;

            IsBusy = true; StatusMessage = $"Aplicando perfil {GetModeName(mode)}...";
            try
            {
                var result = await performanceProfileService.ApplyProfileAsync(mode);
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
            _ = ExecuteRunToolAsync(param);
        }

        private async Task ExecuteRunToolAsync(object? param)
        {
            if (param is not string tool || string.IsNullOrWhiteSpace(tool)) return;

            try
            {
                ProcessStartInfo startInfo;
                switch (tool)
                {
                    case "ipconfig /flushdns":
                        if (_dialogService == null || !await _dialogService.ShowConfirmation(
                                "Se vaciará la caché DNS local. Windows volverá a resolver los dominios cuando se necesiten. ¿Deseas continuar?",
                                "Confirmar limpieza DNS"))
                            return;
                        startInfo = new ProcessStartInfo("ipconfig.exe", "/flushdns") { UseShellExecute = true };
                        break;

                    case "netsh winsock reset":
                        if (_dialogService == null || !await _dialogService.ShowConfirmation(
                                "Se restablecerá el catálogo Winsock. Esto puede interrumpir conexiones y requerir reiniciar Windows. ¿Deseas continuar?",
                                "Confirmar restablecimiento de red"))
                            return;
                        startInfo = new ProcessStartInfo("netsh.exe", "winsock reset")
                        {
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        break;

                    case "msdt.exe -id NetworkDiagnosticsWeb":
                        startInfo = new ProcessStartInfo("msdt.exe", "-id NetworkDiagnosticsWeb") { UseShellExecute = true };
                        break;

                    case "ms-settings:windowsupdate":
                    case "windowsdefender://":
                        startInfo = new ProcessStartInfo(tool) { UseShellExecute = true };
                        break;

                    case "appwiz.cpl":
                    case "cmd.exe":
                    case "devmgmt.msc":
                    case "diskmgmt.msc":
                    case "dxdiag.exe":
                    case "eventvwr.msc":
                    case "gpedit.msc":
                    case "msconfig.exe":
                    case "msinfo32":
                    case "perfmon.msc":
                    case "powercfg.cpl":
                    case "powershell.exe":
                    case "regedit.exe":
                    case "resmon.exe":
                    case "services.msc":
                        startInfo = new ProcessStartInfo(tool) { UseShellExecute = true };
                        break;

                    default:
                        WriteLog($"Se bloqueó un destino de herramienta no permitido: {tool}");
                        GeneralStatusMessage = "La herramienta solicitada no está en la lista permitida.";
                        return;
                }

                Process.Start(startInfo);
                WriteLog($"Herramienta ejecutada correctamente: {tool}");
            }
            catch (Exception ex)
            {
                WriteLog($"Error al ejecutar la herramienta: {tool}", ex);
                GeneralStatusMessage = $"No se pudo iniciar la herramienta: {ex.Message}";
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
        // 7b. COMANDOS DE MANTENIMIENTO Y OPTIMIZACIÓN
        // ==========================================
        private void ExecuteFlushDns()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c ipconfig /flushdns",
                    UseShellExecute = true,
                    Verb = "runas"
                });
                GeneralStatusMessage = "DNS limpiado correctamente.";
                WriteLog("DNS flush ejecutado.");
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = $"Error al limpiar DNS: {ex.Message}";
                WriteLog("Error al limpiar DNS", ex);
            }
        }

        private void ExecuteCleanPrefetch()
        {
            try
            {
                string prefetchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                if (Directory.Exists(prefetchPath))
                {
                    int count = 0;
                    foreach (var file in new DirectoryInfo(prefetchPath).EnumerateFiles("*.pf"))
                    {
                        try { file.Delete(); count++; } catch { }
                    }
                    GeneralStatusMessage = $"{count} archivos Prefetch eliminados.";
                    WriteLog($"Prefetch limpiado: {count} archivos eliminados.");
                }
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = $"Error al limpiar Prefetch: {ex.Message}";
                WriteLog("Error al limpiar Prefetch", ex);
            }
        }

        private void ExecuteRebuildSearchIndex()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "control",
                    Arguments = "srchadmin.dll",
                    UseShellExecute = true
                });
                GeneralStatusMessage = "Abriendo configuración de índice de búsqueda.";
                WriteLog("Índice de búsqueda - abierto panel de configuración.");
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = $"Error al abrir índice de búsqueda: {ex.Message}";
                WriteLog("Error al abrir índice de búsqueda", ex);
            }
        }

        private void ExecuteResetNetwork()
        {
            try
            {
                var result = MessageBox.Show(
                    "Se reiniciarán los componentes de red (Winsock, TCP/IP). ¿Desea continuar?",
                    "Reiniciar Red",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c netsh winsock reset && netsh int ip reset && ipconfig /release && ipconfig /renew && ipconfig /flushdns",
                    UseShellExecute = true,
                    Verb = "runas"
                });
                GeneralStatusMessage = "Red reiniciada. Puede ser necesario reiniciar.";
                WriteLog("Red reiniciada con netsh.");
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = $"Error al reiniciar red: {ex.Message}";
                WriteLog("Error al reiniciar red", ex);
            }
        }

        private void ExecuteAnalyzeDisk()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "dfrgui.exe",
                    UseShellExecute = true
                });
                GeneralStatusMessage = "Abriendo optimizador de discos.";
                WriteLog("Optimizador de discos abierto.");
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = $"Error al abrir optimizador: {ex.Message}";
                WriteLog("Error al abrir optimizador de discos", ex);
            }
        }

        private void ExecuteAnalyze(object? parameter)
        {
            CurrentSection = AppSection.MapaEspacio;
            WriteLog("Navegando al mapa de espacio del disco.");
        }

        private void ExecuteReduceBackgroundProcesses()
        {
            try
            {
                var result = MessageBox.Show(
                    "Se intentará reducir los procesos en segundo plano. Algunos servicios pueden reiniciarse. ¿Desea continuar?",
                    "Reducir procesos",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                int count = 0;
                var nonCritical = Process.GetProcesses()
                    .Where(p => !s_criticalProcessNames.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(p => p.TotalProcessorTime.TotalSeconds)
                    .Take(5)
                    .ToList();

                foreach (var p in nonCritical)
                {
                    try { p.Kill(); p.WaitForExit(1000); count++; } catch { }
                }
                GeneralStatusMessage = $"{count} procesos reducidos.";
                WriteLog($"{count} procesos en segundo plano reducidos.");
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = $"Error al reducir procesos: {ex.Message}";
                WriteLog("Error al reducir procesos", ex);
            }
        }

        private static readonly HashSet<string> s_criticalProcessNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "System", "Idle", "Registry", "smss", "csrss", "wininit", "winlogon",
            "services", "lsass", "svchost", "dwm", "fontdrvhost", "NisSrv",
            "explorer", "sihost", "RuntimeBroker", "ShellExperienceHost"
        };

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
        // 9. BACKUP & RESTORE MÉTODOS
        // ==========================================
        private void SelectBackupDestination()
        {
            // Usar la ruta por defecto en el escritorio
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "WPC-SutilBox Backups", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            var result = MessageBox.Show(
                $"¿Deseas usar la carpeta por defecto para el backup?\n\n{defaultPath}",
                "Confirmar carpeta de destino",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                BackupDestinationPath = defaultPath;
            }
        }

        private async Task StartBackupAsync()
        {
            if (_backupService == null || string.IsNullOrWhiteSpace(BackupDestinationPath))
            {
                GeneralStatusMessage = "Selecciona una carpeta de destino.";
                return;
            }

            if (IsBackupRunning) return;

            _backupCts = new CancellationTokenSource();
            IsBackupRunning = true;
            BackupProgress = 0;
            BackupStatusMessage = "Iniciando copia de seguridad...";
            BackupProgressText = "Preparando...";
            BackupCompleted = false;
            BackupResultMessage = string.Empty;

            try
            {
                var progress = new Progress<(int Percentage, string Message)>(p =>
                {
                    BackupProgress = p.Percentage;
                    BackupStatusMessage = p.Message;
                    BackupProgressText = $"{p.Percentage}% completado";
                });

                var result = await _backupService.CreateFullBackupAsync(BackupDestinationPath, progress, _backupCts.Token);

                if (result.Success)
                {
                    BackupCompleted = true;
                    BackupResultMessage = result.Message;
                    GeneralStatusMessage = "Copia de seguridad completada con éxito.";
                    WriteLog($"Backup completado: {result.Message}");
                    await RefreshBackupsAsync();
                }
                else
                {
                    BackupStatusMessage = result.Message;
                    GeneralStatusMessage = result.Message;
                    WriteLog($"Backup fallido: {result.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                BackupStatusMessage = "Copia de seguridad cancelada.";
                GeneralStatusMessage = "Copia de seguridad cancelada.";
                WriteLog("Backup cancelado por el usuario.");
            }
            catch (Exception ex)
            {
                BackupStatusMessage = $"Error: {ex.Message}";
                GeneralStatusMessage = $"Error: {ex.Message}";
                WriteLog("Error en backup", ex);
            }
            finally
            {
                _backupCts?.Dispose();
                _backupCts = null;
                IsBackupRunning = false;
            }
        }

        private async Task RefreshBackupsAsync()
        {
            if (_backupService == null) return;
            var backups = await _backupService.GetExistingBackupsAsync();
            ExistingBackups.Clear();
            foreach (var backup in backups)
                ExistingBackups.Add(backup);
        }

        private void OpenBackupFolder(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                WriteLog($"Error al abrir carpeta de backup: {ex.Message}", ex);
            }
        }

        private async Task DeleteBackupAsync(object? parameter)
        {
            if (_backupService == null || parameter is not string path) return;

            var result = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar este backup?\n\n{path}",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var success = await _backupService.DeleteBackupAsync(path);
            if (success)
            {
                WriteLog($"Backup eliminado: {path}");
                await RefreshBackupsAsync();
            }
        }

        private void SelectBackupFolder()
        {
            // Refrescar la lista de backups y dejar que el usuario seleccione de la lista
            _ = RefreshBackupsAsync();
            MessageBox.Show(
                "Selecciona un backup de la lista 'Backups Disponibles' más abajo, o haz clic en 'Actualizar' para refrescar la lista.",
                "Seleccionar backup",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async void SelectBackupByPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            SelectedBackupPath = path;
            RestoreCompleted = false;
            RestoreResultMessage = string.Empty;

            if (_restoreService != null)
            {
                var manifest = await _restoreService.LoadBackupManifestAsync(path);
                if (manifest != null)
                {
                    BackupInfo = manifest.CreatedAt;
                    SoftwareCount = manifest.InstalledSoftware.Count;
                    WifiCount = manifest.WifiProfiles.Count;
                    BackupSize = manifest.TotalSizeBytes;
                }
            }
        }

        private async Task StartRestoreAsync()
        {
            if (_restoreService == null || string.IsNullOrWhiteSpace(SelectedBackupPath))
            {
                GeneralStatusMessage = "Selecciona un backup para restaurar.";
                return;
            }

            if (IsRestoreRunning) return;

            var result = MessageBox.Show(
                "La restauración requiere privilegios de administrador y puede tardar varios minutos.\n\n¿Deseas continuar?",
                "Confirmar restauración",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _restoreCts = new CancellationTokenSource();
            IsRestoreRunning = true;
            RestoreProgress = 0;
            RestoreStatusMessage = "Iniciando restauración...";
            RestoreProgressText = "Preparando...";
            RestoreCompleted = false;
            RestoreResultMessage = string.Empty;

            try
            {
                var options = new RestoreOptions
                {
                    RestoreDrivers = RestoreDrivers,
                    RestoreWifi = RestoreWifi,
                    ReinstallSoftware = RestoreSoftware,
                    RestoreUserSettings = RestoreUserSettings
                };

                var progress = new Progress<(int Percentage, string Message)>(p =>
                {
                    RestoreProgress = p.Percentage;
                    RestoreStatusMessage = p.Message;
                    RestoreProgressText = $"{p.Percentage}% completado";
                });

                var restoreResult = await _restoreService.RestoreFromBackupAsync(SelectedBackupPath, options, progress, _restoreCts.Token);

                if (restoreResult.Success)
                {
                    RestoreCompleted = true;
                    RestoreResultMessage = restoreResult.Message;
                    GeneralStatusMessage = "Restauración completada con éxito.";
                    WriteLog($"Restauración completada: {restoreResult.Message}");
                }
                else
                {
                    RestoreStatusMessage = restoreResult.Message;
                    GeneralStatusMessage = restoreResult.Message;
                    WriteLog($"Restauración fallida: {restoreResult.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                RestoreStatusMessage = "Restauración cancelada.";
                GeneralStatusMessage = "Restauración cancelada.";
                WriteLog("Restauración cancelada por el usuario.");
            }
            catch (Exception ex)
            {
                RestoreStatusMessage = $"Error: {ex.Message}";
                GeneralStatusMessage = $"Error: {ex.Message}";
                WriteLog("Error en restauración", ex);
            }
            finally
            {
                _restoreCts?.Dispose();
                _restoreCts = null;
                IsRestoreRunning = false;
            }
        }

        // ==========================================
        // 10. UTILIDAD DE REGISTRO (LOGS)
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
