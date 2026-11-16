using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class CleanupViewModel : ViewModelBase
    {
        private readonly ISystemMaintenanceService _maintenanceService;
        private readonly IDialogService _dialogService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Selecciona las categorías a limpiar.";
        private bool _cleanSystemTemp = true;
        private bool _cleanRecycleBin;
        private bool _cleanBrowserCache;
        private bool _cleanWindowsUpdate;
        private bool _cleanThumbnails;
        private bool _cleanEventLogs;

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public bool CleanSystemTemp { get => _cleanSystemTemp; set => SetProperty(ref _cleanSystemTemp, value); }
        public bool CleanRecycleBin { get => _cleanRecycleBin; set => SetProperty(ref _cleanRecycleBin, value); }
        public bool CleanBrowserCache { get => _cleanBrowserCache; set => SetProperty(ref _cleanBrowserCache, value); }
        public bool CleanWindowsUpdate { get => _cleanWindowsUpdate; set => SetProperty(ref _cleanWindowsUpdate, value); }
        public bool CleanThumbnails { get => _cleanThumbnails; set => SetProperty(ref _cleanThumbnails, value); }
        public bool CleanEventLogs { get => _cleanEventLogs; set => SetProperty(ref _cleanEventLogs, value); }

        public ObservableCollection<CleanupCandidate> CleanupCandidates { get; } = new();
        public int CleanupCandidateCount => CleanupCandidates.Count;
        public long CleanupEstimatedBytes => CleanupCandidates.Sum(c => c.SizeBytes);
        public string FormattedEstimatedBytes => FormatBytes(CleanupEstimatedBytes);

        public ICommand PreviewCommand { get; }
        public ICommand CleanCommand { get; }

        public CleanupViewModel(ISystemMaintenanceService maintenanceService, IDialogService dialogService, ILogService logService)
        {
            _maintenanceService = maintenanceService;
            _dialogService = dialogService;
            _logService = logService;

            PreviewCommand = new AsyncRelayCommand(_ => PreviewCleanupAsync());
            CleanCommand = new AsyncRelayCommand(_ => ExecuteCleanAsync());
        }

        public async Task PreviewCleanupAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Analizando archivos temporales...";

            try
            {
                var options = new CleaningOptions
                {
                    CleanSystemTemp = CleanSystemTemp,
                    CleanRecycleBin = CleanRecycleBin,
                    CleanBrowserCache = CleanBrowserCache,
                    CleanWindowsUpdate = CleanWindowsUpdate,
                    CleanThumbnails = CleanThumbnails,
                    CleanEventLogs = CleanEventLogs
                };

                var candidates = await _maintenanceService.PreviewTemporaryFilesAsync(options);
                CleanupCandidates.Clear();
                foreach (var candidate in candidates) CleanupCandidates.Add(candidate);

                StatusMessage = $"{CleanupCandidateCount} archivos encontrados ({FormattedEstimatedBytes}).";
                _logService.Info($"Preview limpieza: {CleanupCandidateCount} candidatos, {FormattedEstimatedBytes}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al analizar: {ex.Message}";
                _logService.Error("Error en preview de limpieza", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteCleanAsync()
        {
            if (IsBusy || CleanupCandidates.Count == 0) return;

            var confirmed = await _dialogService.ShowConfirmation(
                $"Se eliminarán {CleanupCandidateCount} archivos ({FormattedEstimatedBytes}). ¿Deseas continuar?",
                "Confirmar limpieza");

            if (!confirmed) return;

            IsBusy = true;
            StatusMessage = "Limpiando archivos...";

            try
            {
                var result = await _maintenanceService.CleanTemporaryFilesAsync(CleanupCandidates.ToList());
                StatusMessage = result.ToString();
                CleanupCandidates.Clear();
                _logService.Info($"Limpieza completada: {result}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al limpiar: {ex.Message}";
                _logService.Error("Error en limpieza", ex);
            }
            finally
            {
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
    }
}
