using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;

namespace Wpc_SutilBox.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly IHistoryService _historyService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Cargando historial...";
        private HistoryCategory? _selectedCategory;

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public HistoryCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    _ = LoadEntriesAsync();
            }
        }

        public ObservableCollection<HistoryEntry> Entries { get; } = new();
        public int ReversibleCount => Entries.Count(e => e.IsReversible && !e.WasReverted);
        public int RevertedCount => Entries.Count(e => e.WasReverted);

        public ICommand RefreshCommand { get; }
        public ICommand RevertCommand { get; }
        public ICommand ClearRevertedCommand { get; }
        public ICommand ClearAllCommand { get; }

        public HistoryViewModel(IHistoryService historyService, ILogService logService)
        {
            _historyService = historyService;
            _logService = logService;

            RefreshCommand = new AsyncRelayCommand(_ => LoadEntriesAsync());
            RevertCommand = new AsyncRelayCommand(async p => await RevertEntryAsync(p));
            ClearRevertedCommand = new AsyncRelayCommand(_ => ClearRevertedAsync());
            ClearAllCommand = new AsyncRelayCommand(_ => ClearAllAsync());
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadEntriesAsync();
        }

        public async Task LoadEntriesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Cargando historial...";

            try
            {
                var entries = SelectedCategory.HasValue
                    ? await _historyService.GetEntriesByCategoryAsync(SelectedCategory.Value)
                    : await _historyService.GetEntriesAsync();

                Entries.Clear();
                foreach (var entry in entries) Entries.Add(entry);

                StatusMessage = $"{Entries.Count} entradas en el historial ({ReversibleCount} reversibles, {RevertedCount} revertidas).";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar historial: {ex.Message}";
                _logService.Error("Error al cargar historial", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RevertEntryAsync(object? parameter)
        {
            if (parameter is not HistoryEntry entry) return;

            try
            {
                var success = await _historyService.RevertEntryAsync(entry.Id);
                if (success)
                {
                    entry.WasReverted = true;
                    StatusMessage = $"{entry.Title} se revirtió correctamente.";
                    _logService.Info($"History reverted: {entry.Title}");
                }
                else
                {
                    StatusMessage = $"No se pudo revertir {entry.Title}; no existe una acción de reversión implementada.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al revertir: {ex.Message}";
                _logService.Error("Error al revertir entrada del historial", ex);
            }
        }

        public async Task ClearRevertedAsync()
        {
            try
            {
                await _historyService.ClearRevertedAsync();
                await LoadEntriesAsync();
                StatusMessage = "Entradas revertidas eliminadas.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public async Task ClearAllAsync()
        {
            try
            {
                await _historyService.ClearAsync();
                Entries.Clear();
                StatusMessage = "Historial limpiado.";
                _logService.Info("History cleared");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
    }
}
