using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class DriversViewModel : ViewModelBase
    {
        private readonly IDriverService _driverService;
        private readonly ILogService _logService;
        private CancellationTokenSource? _exportCts;

        private bool _isBusy;
        private bool _isExporting;
        private string _statusMessage = "Cargando información de drivers...";
        private int _exportProgress;
        private string _exportStatusMessage = string.Empty;

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public bool IsExporting { get => _isExporting; private set => SetProperty(ref _isExporting, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public int ExportProgress { get => _exportProgress; private set => SetProperty(ref _exportProgress, value); }
        public string ExportStatusMessage { get => _exportStatusMessage; private set => SetProperty(ref _exportStatusMessage, value); }

        public ObservableCollection<DriverInfo> DriversWithProblems { get; } = new();
        public ObservableCollection<DriverInfo> AllDrivers { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand ExportDriversCommand { get; }
        public ICommand CancelExportCommand { get; }

        public DriversViewModel(IDriverService driverService, ILogService logService)
        {
            _driverService = driverService;
            _logService = logService;

            RefreshCommand = new AsyncRelayCommand(_ => LoadDriversAsync());
            ExportDriversCommand = new AsyncRelayCommand(_ => ExportDriversAsync());
            CancelExportCommand = new RelayCommand(_ => _exportCts?.Cancel());
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadDriversAsync();
        }

        public async Task LoadDriversAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Cargando información de drivers...";

            try
            {
                var drivers = await _driverService.GetDriversWithProblemsAsync();
                DriversWithProblems.Clear();
                foreach (var driver in drivers) DriversWithProblems.Add(driver);

                StatusMessage = $"{DriversWithProblems.Count} drivers con problemas detectados.";
                _logService.Info($"Drivers con problemas: {DriversWithProblems.Count}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar drivers: {ex.Message}";
                _logService.Error("Error al cargar drivers", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExportDriversAsync()
        {
            if (IsExporting) return;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var destinationPath = Path.Combine(desktopPath, "WPC-SutilBox Backups", "Drivers", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            var result = MessageBox.Show(
                $"Se exportarán los controladores de terceros mediante DISM.\n\nDestino:\n{destinationPath}\n\n¿Deseas continuar?",
                "Confirmar copia de seguridad de controladores",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _exportCts = new CancellationTokenSource();
            IsExporting = true;
            ExportProgress = 0;
            ExportStatusMessage = "Preparando exportación de controladores...";

            try
            {
                var progress = new Progress<(int Percentage, string Message)>(update =>
                {
                    ExportProgress = update.Percentage;
                    ExportStatusMessage = update.Message;
                });

                var exportResult = await _driverService.ExportDriversAsync(destinationPath, progress, _exportCts.Token);
                ExportStatusMessage = exportResult.Success
                    ? $"Copia creada en: {destinationPath}"
                    : exportResult.Message;
                StatusMessage = exportResult.Success
                    ? "Copia de seguridad de controladores completada."
                    : exportResult.Message;
                _logService.Info($"Backup de drivers: {StatusMessage}");
            }
            catch (OperationCanceledException)
            {
                ExportStatusMessage = "Exportación cancelada.";
                StatusMessage = "Exportación cancelada.";
            }
            catch (Exception ex)
            {
                ExportStatusMessage = "No se pudo exportar la copia de seguridad.";
                StatusMessage = ExportStatusMessage;
                _logService.Error("Error exportando backup de drivers", ex);
            }
            finally
            {
                _exportCts?.Dispose();
                _exportCts = null;
                IsExporting = false;
            }
        }
    }
}
