using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

        private int _activeTcpConnections;
        private string _systemStatus = string.Empty;
        private string _applicationTitle = string.Empty;
        private string _currentSection = "Dashboard";

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

        // ==========================================
        // 4. CONSTRUCTOR PRINCIPAL
        // ==========================================
        public MainViewModel(
            ITemperatureMonitorService? temperatureMonitorService = null,
            IDiskHealthService? diskHealthService = null)
        {
            _temperatureMonitorService = temperatureMonitorService;
            _diskHealthService = diskHealthService;

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

            NavigateCommand = new RelayCommand(param =>
            {
                if (param is string section && !string.IsNullOrWhiteSpace(section))
                {
                    CurrentSection = section;
                    WriteLog($"Navegando a la sección: {section}");
                }
            });

            // Escribe un log de prueba al iniciar la app para verificar la ruta
            WriteLog("Aplicación iniciada correctamente en modo Debug.");
        }

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
            await Task.CompletedTask;
        }

        public async Task UpdateThermalAsync()
        {
            if (_temperatureMonitorService != null)
            {
                var temp = await _temperatureMonitorService.GetCpuTemperatureCAsync();
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
        public void MinimizeToTray()
        {
            IsWindowVisible = false;
        }

        public async Task PrepareForShutdownAsync()
        {
            await Task.CompletedTask;
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