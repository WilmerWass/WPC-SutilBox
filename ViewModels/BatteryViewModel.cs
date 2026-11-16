using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class BatteryViewModel : ViewModelBase
    {
        private readonly IBatteryService _batteryService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Cargando información de batería...";
        private BatteryInfo _batteryInfo = new();

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public BatteryInfo BatteryInfo { get => _batteryInfo; private set => SetProperty(ref _batteryInfo, value); }

        public ICommand RefreshCommand { get; }

        public BatteryViewModel(IBatteryService batteryService, ILogService logService)
        {
            _batteryService = batteryService;
            _logService = logService;

            RefreshCommand = new AsyncRelayCommand(_ => LoadBatteryInfoAsync());
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadBatteryInfoAsync();
        }

        public async Task LoadBatteryInfoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Obteniendo información de batería...";

            try
            {
                var info = await _batteryService.GetBatteryStatusAsync();
                if (info.IsReadSuccessful)
                {
                    BatteryInfo = info;
                    StatusMessage = "Información de batería actualizada.";
                }
                else
                {
                    StatusMessage = "No se pudo leer la información de la batería. Es posible que el equipo no tenga batería.";
                }
                _logService.Info($"Batería: {StatusMessage}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener información de batería: {ex.Message}";
                _logService.Error("Error al obtener información de batería", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
