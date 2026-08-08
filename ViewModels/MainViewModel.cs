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

        // ==========================================
        // 2. PROPIEDADES DE ENLACE (BINDINGS)
        // ==========================================
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

        // ==========================================
        // 3. COMANDOS DE ACCIÓN Y NAVEGACIÓN
        // ==========================================
        public ICommand OpenDiscordCommand { get; }
        public ICommand OpenCleanmgrCommand { get; }
        public ICommand CleanTempFilesCommand { get; }
        public ICommand OpenLogFolderCommand { get; }
        public ICommand NavigateCommand { get; }

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

            OpenDiscordCommand = new RelayCommand(ExecuteOpenDiscord);
            OpenCleanmgrCommand = new RelayCommand(ExecuteOpenCleanmgr);
            CleanTempFilesCommand = new AsyncRelayCommand(ExecuteCleanTempFilesAsync);
            OpenLogFolderCommand = new RelayCommand(ExecuteOpenLogFolder);

            NavigateCommand = new RelayCommand(param =>
            {
                if (param is string section && !string.IsNullOrWhiteSpace(section))
                {
                    CurrentSection = section;
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