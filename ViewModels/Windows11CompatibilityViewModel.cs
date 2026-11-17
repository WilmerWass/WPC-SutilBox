using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public sealed class Windows11CompatibilityViewModel : ViewModelBase
    {
        private readonly IWindows11CompatibilityService _service;
        private readonly ILogService _logService;
        private bool _isBusy;
        private string _statusMessage = "Ejecuta el análisis para consultar los requisitos disponibles.";

        public ObservableCollection<CompatibilityCheck> Checks { get; } = new();
        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public ICommand RefreshCommand { get; }

        public Windows11CompatibilityViewModel(IWindows11CompatibilityService service, ILogService logService)
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
            StatusMessage = "Consultando requisitos de forma segura...";
            try
            {
                var checks = await _service.CheckAsync();
                Checks.Clear();
                foreach (var check in checks)
                    Checks.Add(check);
                StatusMessage = "Análisis informativo completado. El resultado no garantiza que la actualización esté disponible para este equipo.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo completar el análisis: {ex.Message}";
                _logService.Error("Error comprobando compatibilidad con Windows 11", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
