using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class RestorePointViewModel : ViewModelBase
    {
        private readonly IRestorePointService _restorePointService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Cargando puntos de restauración...";
        private string _restorePointName = "WPC-SutilBox Backup";

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public string RestorePointName { get => _restorePointName; set => SetProperty(ref _restorePointName, value); }

        public ICommand CreateCommand { get; }
        public ICommand RefreshCommand { get; }

        public RestorePointViewModel(IRestorePointService restorePointService, ILogService logService)
        {
            _restorePointService = restorePointService;
            _logService = logService;

            CreateCommand = new AsyncRelayCommand(_ => CreateRestorePointAsync());
            RefreshCommand = new AsyncRelayCommand(_ => RefreshStatusAsync());
        }

        public async Task CreateRestorePointAsync()
        {
            if (IsBusy) return;

            var name = string.IsNullOrWhiteSpace(RestorePointName)
                ? $"WPC-SutilBox - {DateTime.Now:yyyy-MM-dd HH:mm}"
                : RestorePointName;

            var result = MessageBox.Show(
                $"Se creará un punto de restauración con el nombre: {name}\n\n¿Deseas continuar?",
                "Crear punto de restauración",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsBusy = true;
            StatusMessage = "Creando punto de restauración...";

            try
            {
                var createResult = await _restorePointService.CreateRestorePointAsync(name);
                StatusMessage = createResult.Message;
                _logService.Info($"Restore point: {createResult.Message}");

                if (createResult.Success)
                {
                    MessageBox.Show("Punto de restauración creado correctamente.", "WPC-SutilBox", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al crear punto de restauración: {ex.Message}";
                _logService.Error("Error al crear punto de restauración", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RefreshStatusAsync()
        {
            IsBusy = true;
            StatusMessage = "Consultando último punto de restauración...";

            try
            {
                var last = await _restorePointService.GetLastRestorePointAsync();
                if (last.Date.HasValue)
                {
                    StatusMessage = $"Último punto: {last.Name} ({last.Date.Value:yyyy-MM-dd HH:mm})";
                }
                else
                {
                    StatusMessage = "No se encontraron puntos de restauración.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al consultar: {ex.Message}";
                _logService.Error("Error al consultar puntos de restauración", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
