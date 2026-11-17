using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public sealed class UsbStorageViewModel : ViewModelBase
    {
        private readonly IPeripheralDiagnosticService _service;
        private readonly ILogService _logService;
        private bool _isBusy;
        private string _statusMessage = "Lista de unidades USB sin consultar.";

        public ObservableCollection<UsbStorageDiagnostic> Devices { get; } = new();
        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public ICommand RefreshCommand { get; }

        public UsbStorageViewModel(IPeripheralDiagnosticService service, ILogService logService)
        {
            _service = service;
            _logService = logService;
            RefreshCommand = new AsyncRelayCommand(_ => RefreshAsync());
        }

        public async Task OnNavigatedToAsync() => await RefreshAsync();

        public async Task RefreshAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Consultando unidades USB de almacenamiento...";
            try
            {
                var devices = await _service.GetUsbStorageDevicesAsync();
                Devices.Clear();
                foreach (var device in devices)
                    Devices.Add(device);

                StatusMessage = Devices.Count == 0
                    ? "No se detectaron unidades USB de almacenamiento conectadas."
                    : $"{Devices.Count} unidades USB detectadas. La capacidad es nominal y no se ha probado la capacidad real.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al consultar unidades USB: {ex.Message}";
                _logService.Error("Error al actualizar el diagnóstico USB", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
