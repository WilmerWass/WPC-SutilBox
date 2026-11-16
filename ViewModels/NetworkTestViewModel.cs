using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class NetworkTestViewModel : ViewModelBase
    {
        private readonly INetworkTestService _networkTestService;
        private readonly ILogService _logService;
        private CancellationTokenSource? _testCts;

        private bool _isBusy;
        private string _statusMessage = "Listo para probar la conexión.";
        private string _pingResult = "--";
        private string _dnsResult = "--";
        private string _downloadResult = "--";
        private string _uploadResult = "--";
        private int _progress;

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public string PingResult { get => _pingResult; private set => SetProperty(ref _pingResult, value); }
        public string DnsResult { get => _dnsResult; private set => SetProperty(ref _dnsResult, value); }
        public string DownloadResult { get => _downloadResult; private set => SetProperty(ref _downloadResult, value); }
        public string UploadResult { get => _uploadResult; private set => SetProperty(ref _uploadResult, value); }
        public int Progress { get => _progress; private set => SetProperty(ref _progress, value); }

        public ICommand RunTestCommand { get; }
        public ICommand CancelCommand { get; }

        public NetworkTestViewModel(INetworkTestService networkTestService, ILogService logService)
        {
            _networkTestService = networkTestService;
            _logService = logService;
            RunTestCommand = new AsyncRelayCommand(_ => RunNetworkTestAsync());
            CancelCommand = new RelayCommand(_ => _testCts?.Cancel());
        }

        public async Task RunNetworkTestAsync()
        {
            if (IsBusy) return;
            _testCts = new CancellationTokenSource();
            IsBusy = true;
            Progress = 0;
            StatusMessage = "Iniciando prueba de red...";
            PingResult = "--";
            DnsResult = "--";
            DownloadResult = "--";
            UploadResult = "--";

            try
            {
                // Ping
                StatusMessage = "Probando latencia (ping)...";
                Progress = 10;
                var pingMs = await _networkTestService.PingAsync("8.8.8.8", cancellationToken: _testCts.Token);
                PingResult = pingMs.HasValue ? $"{pingMs.Value:F0} ms" : "Sin respuesta";
                _logService.Info($"Ping: {PingResult}");

                // DNS
                StatusMessage = "Probando resolución DNS...";
                Progress = 30;
                var dns = await _networkTestService.ResolveDnsAsync("www.google.com", _testCts.Token);
                DnsResult = $"{dns.Server} ({dns.Ms:F0} ms)";
                _logService.Info($"DNS: {DnsResult}");

                // Download
                StatusMessage = "Probando velocidad de descarga...";
                Progress = 50;
                var dl = await _networkTestService.MeasureDownloadSpeedAsync(_testCts.Token);
                DownloadResult = dl.HasValue ? $"{dl.Value:F2} Mbps" : "No disponible";
                _logService.Info($"Descarga: {DownloadResult}");

                // Upload
                StatusMessage = "Probando velocidad de subida...";
                Progress = 75;
                var ul = await _networkTestService.MeasureUploadSpeedAsync(_testCts.Token);
                UploadResult = ul.HasValue ? $"{ul.Value:F2} Mbps" : "No disponible";
                _logService.Info($"Subida: {UploadResult}");

                Progress = 100;
                StatusMessage = "Prueba de red completada.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Prueba cancelada.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _logService.Error("Error en prueba de red", ex);
            }
            finally
            {
                _testCts?.Dispose();
                _testCts = null;
                IsBusy = false;
            }
        }
    }
}
