using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
        private CancellationTokenSource? _operationCancellation;
        private string _statusMessage = "Analiza los archivos temporales autorizados para ver cuánto espacio puedes liberar.";

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value)) OnPropertyChanged(nameof(CanClean));
            }
        }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        public ObservableCollection<CleanupCandidate> CleanupCandidates { get; } = new();
        public int CleanupCandidateCount => CleanupCandidates.Count;
        public bool HasCleanupCandidates => CleanupCandidates.Count > 0;
        public bool CanClean => HasCleanupCandidates && !IsBusy;
        public long CleanupEstimatedBytes => CleanupCandidates.Sum(c => c.SizeBytes);
        public string FormattedEstimatedBytes => FormatBytes(CleanupEstimatedBytes);

        public ICommand PreviewCommand { get; }
        public ICommand CleanCommand { get; }
        public ICommand CancelCommand { get; }

        public CleanupViewModel(ISystemMaintenanceService maintenanceService, IDialogService dialogService, ILogService logService)
        {
            _maintenanceService = maintenanceService;
            _dialogService = dialogService;
            _logService = logService;

            PreviewCommand = new AsyncRelayCommand(_ => PreviewCleanupAsync());
            CleanCommand = new AsyncRelayCommand(
                ExecuteCleanAsync,
                selectedItems => !IsBusy && selectedItems is IList selectedList && selectedList.Count > 0);
            CancelCommand = new RelayCommand(_ => _operationCancellation?.Cancel(), _ => IsBusy);
        }

        public async Task PreviewCleanupAsync()
        {
            if (IsBusy) return;
            using var cancellation = new CancellationTokenSource();
            _operationCancellation = cancellation;
            IsBusy = true;
            StatusMessage = "Analizando archivos temporales...";

            try
            {
                var options = new CleaningOptions { CleanSystemTemp = true };

                var candidates = await _maintenanceService.PreviewTemporaryFilesAsync(options, cancellation.Token);
                CleanupCandidates.Clear();
                foreach (var candidate in candidates) CleanupCandidates.Add(candidate);
                OnPropertyChanged(nameof(CleanupCandidateCount));
                OnPropertyChanged(nameof(HasCleanupCandidates));
                OnPropertyChanged(nameof(CanClean));
                OnPropertyChanged(nameof(CleanupEstimatedBytes));
                OnPropertyChanged(nameof(FormattedEstimatedBytes));

                StatusMessage = $"{CleanupCandidateCount} archivos encontrados ({FormattedEstimatedBytes}).";
                _logService.Info($"Preview limpieza: {CleanupCandidateCount} candidatos, {FormattedEstimatedBytes}");
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Análisis cancelado. No se eliminó ningún archivo.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al analizar: {ex.Message}";
                _logService.Error("Error en preview de limpieza", ex);
            }
            finally
            {
                _operationCancellation = null;
                IsBusy = false;
            }
        }

        public async Task ExecuteCleanAsync(object? selectedItems)
        {
            if (IsBusy) return;

            var selectedCandidates = selectedItems is IList selectedList
                ? selectedList.Cast<CleanupCandidate>().Where(CleanupCandidates.Contains).Distinct().ToList()
                : new List<CleanupCandidate>();
            if (selectedCandidates.Count == 0)
            {
                StatusMessage = "Selecciona uno o varios archivos revisados antes de limpiar.";
                return;
            }

            long selectedBytes = selectedCandidates.Sum(candidate => candidate.SizeBytes);

            var confirmed = await _dialogService.ShowConfirmation(
                $"Se intentarán eliminar los {selectedCandidates.Count} archivos seleccionados ({FormatBytes(selectedBytes)}). ¿Deseas continuar?",
                "Confirmar limpieza");

            if (!confirmed) return;

            IsBusy = true;
            using var cancellation = new CancellationTokenSource();
            _operationCancellation = cancellation;
            StatusMessage = "Limpiando archivos...";

            try
            {
                var result = await _maintenanceService.CleanTemporaryFilesAsync(selectedCandidates, cancellation.Token);
                if (result.FilesFailed == 0)
                {
                    foreach (var candidate in selectedCandidates) CleanupCandidates.Remove(candidate);
                    OnPropertyChanged(nameof(CleanupCandidateCount));
                    OnPropertyChanged(nameof(HasCleanupCandidates));
                    OnPropertyChanged(nameof(CanClean));
                    OnPropertyChanged(nameof(CleanupEstimatedBytes));
                    OnPropertyChanged(nameof(FormattedEstimatedBytes));
                }

                StatusMessage = result.FilesFailed == 0
                    ? result.ToString()
                    : $"{result} Los candidatos seleccionados se conservan para que puedas revisarlos de nuevo.";
                _logService.Info($"Limpieza completada: {result}");
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Limpieza cancelada. Revisa los resultados antes de volver a intentarlo.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al limpiar: {ex.Message}";
                _logService.Error("Error en limpieza", ex);
            }
            finally
            {
                _operationCancellation = null;
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
