using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

#nullable enable

namespace Wpc_SutilBox.ViewModels
{
    /// <summary>
    /// ViewModel principal del módulo "Revisar mi PC" — Beta 1.2.
    /// <br/>
    /// Implementa caché ligera (TTL = 5 min) para consultas WMI y exposición
    /// de colecciones para binding reactivo en la UI.
    /// <br/>
    /// Principio: <b>Cero Consumo Parásito</b> — los contadores solo corren
    /// cuando este ViewModel está activo (invocar OnNavigatedToAsync /
    /// OnNavigatedFromAsync desde el code-behind o el MainViewModel).
    /// </summary>
    public sealed class PcReviewViewModel : ViewModelBase, IDisposable
    {
        // ─── Servicios ────────────────────────────────────────────────────────────
        private readonly ISystemInfoService    _systemInfoService;
        private readonly IDiskHealthService    _diskHealthService;
        private readonly IDriverService        _driverService;
        private readonly IMonitoringService    _monitoringService;
        private readonly ITemperatureMonitorService _temperatureMonitorService;

        // ─── Caché ligera (TTL = 5 min) ──────────────────────────────────────────
        private const int CacheTtlMinutes = 5;
        private DateTime _lastWmiRefresh = DateTime.MinValue;
        private SystemInfo?                  _cachedSystemInfo;
        private IEnumerable<DiskHealthInfo>? _cachedDiskHealth;
        private IEnumerable<DriverInfo>?     _cachedDriverIssues;

        // ─── Estado ────────────────────────────────────────────────────────────────
        private bool   _isBusy;
        private string _statusMessage = string.Empty;
        private double _cpuUsage;
        private double _ramUsage;
        private double _cpuTempC;
        private SystemInfo? _systemInfo;
        private bool _disposed;
        private CancellationTokenSource? _liveCts;

        // ─── Propiedades observables ──────────────────────────────────────────────

        /// <summary>Indica si hay una operación de carga en curso.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        /// <summary>Mensaje de estado para la barra inferior.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>Uso de CPU en porcentaje (0–100).</summary>
        public double CpuUsage
        {
            get => _cpuUsage;
            private set => SetProperty(ref _cpuUsage, value);
        }

        /// <summary>Uso de RAM en porcentaje (0–100).</summary>
        public double RamUsage
        {
            get => _ramUsage;
            private set => SetProperty(ref _ramUsage, value);
        }

        /// <summary>Temperatura del CPU en grados Celsius (si está disponible).</summary>
        public double CpuTempC
        {
            get => _cpuTempC;
            private set => SetProperty(ref _cpuTempC, value);
        }

        /// <summary>Información general del sistema (CPU, RAM, GPU, S.O., etc.).</summary>
        public SystemInfo? SystemInfo
        {
            get => _systemInfo;
            private set => SetProperty(ref _systemInfo, value);
        }

        // ─── Colecciones para binding ─────────────────────────────────────────────

        /// <summary>Listado de discos con su estado SMART.</summary>
        public ObservableCollection<DiskHealthInfo> Disks { get; } = new();

        /// <summary>Listado de drivers con problemas detectados.</summary>
        public ObservableCollection<DriverInfo> DriverIssues { get; } = new();

        /// <summary>Procesos de mayor consumo de RAM (top 30).</summary>
        public ObservableCollection<ProcessInfoDto> Processes { get; } = new();

        // ─── Comandos ─────────────────────────────────────────────────────────────

        /// <summary>Fuerza recarga total (ignora caché WMI).</summary>
        public ICommand RefreshCommand { get; }

        /// <summary>Recarga solo la lista de procesos.</summary>
        public ICommand RefreshProcessesCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────────

        public PcReviewViewModel(
            ISystemInfoService systemInfoService,
            IDiskHealthService diskHealthService,
            IDriverService driverService,
            IMonitoringService monitoringService,
            ITemperatureMonitorService temperatureMonitorService
            )
        {
            _systemInfoService = systemInfoService ?? throw new ArgumentNullException(nameof(systemInfoService));
            _diskHealthService = diskHealthService ?? throw new ArgumentNullException(nameof(diskHealthService));
            _driverService     = driverService     ?? throw new ArgumentNullException(nameof(driverService));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _temperatureMonitorService = temperatureMonitorService ?? throw new ArgumentNullException(nameof(temperatureMonitorService));


            RefreshCommand          = new AsyncRelayCommand(_ => LoadDataAsync(forceRefresh: true));
            RefreshProcessesCommand = new AsyncRelayCommand(_ => LoadProcessesAsync());
        }

        // ─── Ciclo de vida ─────────────────────────────────────────────────────────

        /// <summary>Invocado cuando el usuario navega a esta vista.</summary>
        public async Task OnNavigatedToAsync()
        {
            await _monitoringService.StartAsync();
            await LoadDataAsync();
            StartLiveMonitoring();
        }

        /// <summary>Invocado cuando el usuario abandona esta vista.</summary>
        public async Task OnNavigatedFromAsync()
        {
            StopLiveMonitoring();
            await _monitoringService.StopAsync();
        }

        // ─── Carga de datos ───────────────────────────────────────────────────────

        /// <summary>
        /// Carga / actualiza todos los datos del diagnóstico.
        /// Con <paramref name="forceRefresh"/> = false reutiliza el caché WMI
        /// si no han pasado 5 minutos desde la última consulta.
        /// </summary>
        public async Task LoadDataAsync(bool forceRefresh = false)
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Actualizando diagnóstico…";

            try
            {
                bool cacheExpired = (DateTime.Now - _lastWmiRefresh).TotalMinutes >= CacheTtlMinutes;
                bool refreshWmi   = forceRefresh || cacheExpired;

                // ── Métricas de rendimiento en vivo ──────────────────────────────
                var usage = await _monitoringService.GetSystemUsageAsync();
                CpuUsage = usage.CpuUsage;
                RamUsage = usage.RamUsage;

                try {
                    var temperature = await _temperatureMonitorService.GetCpuTemperatureCAsync();
                    
                    if (temperature.HasValue)
                    CpuTempC = temperature.Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"[PcReviewViewModel] Error obteniendo temperatura: {ex.Message}");
                }

                // ── Datos WMI con caché ──────────────────────────────────────────
                if (refreshWmi)
                {
                    var siTask = _systemInfoService.GetSystemInfoAsync();
                    var dhTask = _diskHealthService.GetDiskHealthAsync();
                    var drTask = _driverService.GetDriversWithProblemsAsync();

                    await Task.WhenAll(siTask, dhTask, drTask);

                    _cachedSystemInfo   = siTask.Result;
                    _cachedDiskHealth   = dhTask.Result;
                    _cachedDriverIssues = drTask.Result;
                    _lastWmiRefresh     = DateTime.Now;
                }

                // ── Aplicar a colecciones ────────────────────────────────────────
                SystemInfo = _cachedSystemInfo;

                Disks.Clear();
                if (_cachedDiskHealth != null)
                    foreach (var d in _cachedDiskHealth) Disks.Add(d);

                DriverIssues.Clear();
                if (_cachedDriverIssues != null)
                    foreach (var d in _cachedDriverIssues) DriverIssues.Add(d);

                await LoadProcessesAsync();

                StatusMessage = $"Última actualización: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PcReviewViewModel] Error en LoadDataAsync: {ex.Message}");
                StatusMessage = "Error al obtener datos. Intente de nuevo.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Recarga únicamente la lista de procesos (sin tocar caché WMI).</summary>
        public async Task LoadProcessesAsync()
        {
            try
            {
                var procs = await Task.Run(() =>
                    Process.GetProcesses()
                           .Where(p => p.WorkingSet64 > 0)
                           .OrderByDescending(p => p.WorkingSet64)
                           .Take(30)
                           .Select(p => new ProcessInfoDto
                           {
                               Pid          = p.Id,
                               Name         = p.ProcessName,
                               WorkingSetMb = p.WorkingSet64 / 1_048_576.0,
                               IsCritical   = s_criticalProcesses.Contains(p.ProcessName),
                           })
                           .ToList());

                Processes.Clear();
                foreach (var p in procs) Processes.Add(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PcReviewViewModel] Error cargando procesos: {ex.Message}");
            }
        }

        // ─── Monitoreo en vivo (CPU / RAM cada 3 s) ──────────────────────────────

        private void StartLiveMonitoring()
        {
            StopLiveMonitoring();
            
            _liveCts = new CancellationTokenSource();
            var token = _liveCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, token);
                        var snap = await _monitoringService.GetGlobalUsageAsync(token);
                        if (snap.CpuUsage.HasValue) CpuUsage = snap.CpuUsage.Value;
                        if (snap.RamUsage.HasValue) RamUsage = snap.RamUsage.Value;
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[PcReviewViewModel] Live-monitoring error: {ex.Message}");
                    }
                }
            }, token);
        }

        private void StopLiveMonitoring()
        {
            _liveCts?.Cancel();
            _liveCts?.Dispose();
            _liveCts = null;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static readonly HashSet<string> s_criticalProcesses = new(StringComparer.OrdinalIgnoreCase)
        {
            "System", "Idle", "Registry", "smss", "csrss", "wininit", "winlogon",
            "services", "lsass", "svchost", "dwm", "fontdrvhost", "NisSrv"
        };

        // ─── IDisposable ──────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopLiveMonitoring();
        }
    }
}
