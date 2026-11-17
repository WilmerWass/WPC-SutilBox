using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;

namespace Wpc_SutilBox.ViewModels
{
    public sealed class BenchmarkViewModel : ViewModelBase
    {
        private readonly IBenchmarkService _service;
        private readonly ILogService _logService;
        private CancellationTokenSource? _benchmarkCancellation;
        private bool _isBusy;
        private string _statusMessage = "Listo para iniciar una prueba breve de CPU y memoria.";
        private string _cpuResult = "—";
        private string _memoryResult = "—";

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    OnPropertyChanged(nameof(CanRun));
            }
        }
        public bool CanRun => !IsBusy;
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public string CpuResult { get => _cpuResult; private set => SetProperty(ref _cpuResult, value); }
        public string MemoryResult { get => _memoryResult; private set => SetProperty(ref _memoryResult, value); }
        public ICommand RunCommand { get; }
        public ICommand CancelCommand { get; }

        public BenchmarkViewModel(IBenchmarkService service, ILogService logService)
        {
            _service = service;
            _logService = logService;
            RunCommand = new AsyncRelayCommand(_ => RunAsync());
            CancelCommand = new RelayCommand(_ => _benchmarkCancellation?.Cancel());
        }

        public async Task RunAsync()
        {
            if (IsBusy) return;
            var cancellation = new CancellationTokenSource();
            _benchmarkCancellation = cancellation;
            IsBusy = true;
            StatusMessage = "Midiendo CPU durante 2 segundos y ancho de banda de memoria...";
            CpuResult = "—";
            MemoryResult = "—";

            try
            {
                var result = await _service.RunAsync(cancellation.Token);
                CpuResult = $"{result.PrimeCount:N0} primos calculados en 2 s";
                MemoryResult = $"{result.MemoryCopyMegabytesPerSecond:N0} MB/s (copia en RAM)";
                StatusMessage = "Prueba finalizada. El resultado depende del uso y la configuración del equipo.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Prueba cancelada.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo completar la prueba: {ex.Message}";
                _logService.Error("Error ejecutando benchmark de CPU y memoria", ex);
            }
            finally
            {
                cancellation.Dispose();
                _benchmarkCancellation = null;
                IsBusy = false;
            }
        }
    }
}
