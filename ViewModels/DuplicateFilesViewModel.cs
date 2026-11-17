using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public sealed class DuplicateFilesViewModel : ViewModelBase
    {
        private readonly IDuplicateFileService _service;
        private readonly ILogService _logService;
        private CancellationTokenSource? _scanCancellation;
        private string _selectedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string _statusMessage = "Elige una carpeta y analiza los archivos. El escaneo no elimina nada.";
        private bool _isBusy;

        public ObservableCollection<DuplicateFileRecord> DuplicateFiles { get; } = new();
        public string SelectedFolder { get => _selectedFolder; set => SetProperty(ref _selectedFolder, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    OnPropertyChanged(nameof(CanScan));
            }
        }
        public bool CanScan => !IsBusy;
        public ICommand BrowseCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand CancelCommand { get; }

        public DuplicateFilesViewModel(IDuplicateFileService service, ILogService logService)
        {
            _service = service;
            _logService = logService;
            BrowseCommand = new RelayCommand(_ => Browse());
            ScanCommand = new AsyncRelayCommand(_ => ScanAsync());
            CancelCommand = new RelayCommand(_ => _scanCancellation?.Cancel());
        }

        private void Browse()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                StatusMessage = "El selector de carpetas está disponible solo en Windows 7 o posterior.";
                return;
            }

            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = SelectedFolder,
                Description = "Selecciona la carpeta que deseas analizar"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SelectedFolder = dialog.SelectedPath;
        }

        public async Task ScanAsync()
        {
            if (IsBusy) return;
            var cancellation = new CancellationTokenSource();
            _scanCancellation = cancellation;
            IsBusy = true;
            DuplicateFiles.Clear();
            StatusMessage = "Preparando el análisis de archivos...";

            var progress = new Progress<string>(message => StatusMessage = message);
            try
            {
                var result = await _service.FindDuplicatesAsync(
                    SelectedFolder,
                    progress,
                    cancellation.Token);

                foreach (var file in result.Files)
                    DuplicateFiles.Add(file);

                StatusMessage = $"Análisis terminado: {result.ScannedFiles:N0} archivos revisados, " +
                    $"{result.DuplicateGroups:N0} grupos duplicados, {result.SkippedItems:N0} elementos omitidos. " +
                    "No se eliminó ningún archivo.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Análisis cancelado. No se eliminó ningún archivo.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo completar el análisis: {ex.Message}";
                _logService.Error("Error buscando archivos duplicados", ex);
            }
            finally
            {
                cancellation.Dispose();
                _scanCancellation = null;
                IsBusy = false;
            }
        }
    }
}
