using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class StartupViewModel : ViewModelBase
    {
        private readonly IStartupService _startupService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Cargando programas de inicio...";

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        public ObservableCollection<StartupItem> StartupItems { get; } = new();
        public int EnabledCount => StartupItems.Count(i => i.IsEnabled);
        public int DisabledCount => StartupItems.Count(i => !i.IsEnabled);

        public ICommand RefreshCommand { get; }
        public ICommand EnableCommand { get; }
        public ICommand DisableCommand { get; }

        public StartupViewModel(IStartupService startupService, ILogService logService)
        {
            _startupService = startupService;
            _logService = logService;

            RefreshCommand = new AsyncRelayCommand(_ => LoadStartupItemsAsync());
            EnableCommand = new AsyncRelayCommand(async p => await ChangeStartupItemAsync(p, true));
            DisableCommand = new AsyncRelayCommand(async p => await ChangeStartupItemAsync(p, false));
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadStartupItemsAsync();
        }

        public async Task LoadStartupItemsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Cargando programas de inicio...";

            try
            {
                var items = await _startupService.GetStartupItemsAsync();
                StartupItems.Clear();
                foreach (var item in items) StartupItems.Add(item);

                StatusMessage = $"{StartupItems.Count} programas de inicio ({EnabledCount} habilitados, {DisabledCount} deshabilitados).";
                _logService.Info($"Startup items: {StartupItems.Count} total");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar programas de inicio: {ex.Message}";
                _logService.Error("Error al cargar programas de inicio", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ChangeStartupItemAsync(object? parameter, bool enabled)
        {
            if (parameter is not StartupItem item) return;

            bool success = enabled
                ? await _startupService.EnableStartupItemAsync(item)
                : await _startupService.DisableStartupItemAsync(item);

            if (success)
            {
                item.IsEnabled = enabled;
                OnPropertyChanged(nameof(StartupItems));
                OnPropertyChanged(nameof(EnabledCount));
                OnPropertyChanged(nameof(DisabledCount));
                StatusMessage = $"{item.Name} {(enabled ? "habilitado" : "deshabilitado")} correctamente.";
                _logService.Info($"Startup item {(enabled ? "enabled" : "disabled")}: {item.Name}");
            }
        }
    }
}
