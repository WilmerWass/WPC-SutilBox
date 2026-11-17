using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public sealed class PeripheralDiagnosticsViewModel : ViewModelBase
    {
        private readonly IPeripheralDiagnosticService _service;
        private readonly ILogService _logService;
        private bool _isBusy;
        private bool _isTesting;
        private bool _testAll;
        private int _testIndex;
        private string _statusMessage = "Usa Actualizar para consultar los dispositivos Plug and Play.";
        private string _activeTestCategory = string.Empty;
        private string _testInstruction = string.Empty;
        private string _lastKey = "Ninguna";
        private string _observedKeys = string.Empty;
        private int _leftClicks;
        private int _rightClicks;
        private int _middleClicks;
        private int _extraClicks;
        private int _wheelSteps;
        private int _pointerMoves;
        private int _screenPatternIndex;
        private string[] _testQueue = Array.Empty<string>();

        private static readonly string[] ScreenPatterns =
        [
            "#FFFFFF", "#000000", "#FF0000", "#00FF00", "#0000FF", "#FFFFFF"
        ];

        public ObservableCollection<PeripheralDiagnostic> Devices { get; } = new();
        public ObservableCollection<string> TestResults { get; } = new();
        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public bool IsTesting
        {
            get => _isTesting;
            private set
            {
                if (SetProperty(ref _isTesting, value))
                {
                    OnPropertyChanged(nameof(IsTestingKeyboard));
                    OnPropertyChanged(nameof(IsTestingMouse));
                    OnPropertyChanged(nameof(IsTestingScreen));
                    OnPropertyChanged(nameof(IsTestingAudio));
                }
            }
        }
        public bool IsTestingKeyboard => IsTesting && ActiveTestCategory == "Teclado";
        public bool IsTestingMouse => IsTesting && ActiveTestCategory == "Mouse";
        public bool IsTestingScreen => IsTesting && ActiveTestCategory == "Pantalla";
        public bool IsTestingAudio => IsTesting && ActiveTestCategory == "Audio";
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public string ActiveTestCategory { get => _activeTestCategory; private set
            {
                if (SetProperty(ref _activeTestCategory, value))
                {
                    OnPropertyChanged(nameof(IsTestingKeyboard));
                    OnPropertyChanged(nameof(IsTestingMouse));
                    OnPropertyChanged(nameof(IsTestingScreen));
                    OnPropertyChanged(nameof(IsTestingAudio));
                }
            }
        }
        public string TestInstruction { get => _testInstruction; private set => SetProperty(ref _testInstruction, value); }
        public string LastKey { get => _lastKey; private set => SetProperty(ref _lastKey, value); }
        public string ObservedKeys { get => _observedKeys; private set => SetProperty(ref _observedKeys, value); }
        public int LeftClicks { get => _leftClicks; private set => SetProperty(ref _leftClicks, value); }
        public int RightClicks { get => _rightClicks; private set => SetProperty(ref _rightClicks, value); }
        public int MiddleClicks { get => _middleClicks; private set => SetProperty(ref _middleClicks, value); }
        public int ExtraClicks { get => _extraClicks; private set => SetProperty(ref _extraClicks, value); }
        public int WheelSteps { get => _wheelSteps; private set => SetProperty(ref _wheelSteps, value); }
        public int PointerMoves { get => _pointerMoves; private set => SetProperty(ref _pointerMoves, value); }
        public System.Windows.Media.Brush ScreenPatternBrush =>
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ScreenPatterns[_screenPatternIndex]));
        public ICommand RefreshCommand { get; }
        public ICommand TestAllCommand { get; }
        public ICommand TestDeviceCommand { get; }
        public ICommand MarkTestPassedCommand { get; }
        public ICommand MarkTestFailedCommand { get; }
        public ICommand NextScreenPatternCommand { get; }
        public ICommand PlayTestToneCommand { get; }

        public PeripheralDiagnosticsViewModel(IPeripheralDiagnosticService service, ILogService logService)
        {
            _service = service;
            _logService = logService;
            RefreshCommand = new AsyncRelayCommand(_ => RefreshAsync());
            TestAllCommand = new RelayCommand(_ => StartAllTests());
            TestDeviceCommand = new RelayCommand(device => StartDeviceTest(device as PeripheralDiagnostic));
            MarkTestPassedCommand = new RelayCommand(_ => CompleteTest(passed: true));
            MarkTestFailedCommand = new RelayCommand(_ => CompleteTest(passed: false));
            NextScreenPatternCommand = new RelayCommand(_ => AdvanceScreenPattern());
            PlayTestToneCommand = new RelayCommand(_ => System.Media.SystemSounds.Beep.Play());
        }

        public async Task OnNavigatedToAsync() => await RefreshAsync();

        public async Task RefreshAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Consultando todos los dispositivos Plug and Play...";
            try
            {
                var devices = await _service.GetDevicesAsync();
                Devices.Clear();
                foreach (var device in devices)
                    Devices.Add(device);

                int issueCount = Devices.Count(device =>
                    !device.Status.Equals("Correcto", StringComparison.OrdinalIgnoreCase)
                    && !device.Status.Equals("OK", StringComparison.OrdinalIgnoreCase));

                StatusMessage = $"{Devices.Count} dispositivos enumerados por Windows · {issueCount} requieren revisión. " +
                    "La consulta no instala controladores ni modifica el sistema.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al consultar dispositivos: {ex.Message}";
                _logService.Error("Error al actualizar el diagnóstico de periféricos", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void RecordKey(string key)
        {
            if (!IsTestingKeyboard || string.IsNullOrWhiteSpace(key))
                return;

            LastKey = key;
            var keys = ObservedKeys.Split(" · ", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!keys.Contains(key, StringComparer.OrdinalIgnoreCase))
                keys.Add(key);
            ObservedKeys = string.Join(" · ", keys);
        }

        public void RecordMouseButton(string button)
        {
            if (!IsTestingMouse) return;
            switch (button)
            {
                case "Izquierdo": LeftClicks++; break;
                case "Derecho": RightClicks++; break;
                case "Central": MiddleClicks++; break;
                case "Adicional": ExtraClicks++; break;
            }
        }

        public void RecordMouseWheel()
        {
            if (IsTestingMouse) WheelSteps++;
        }

        public void RecordPointerMove()
        {
            if (IsTestingMouse) PointerMoves++;
        }

        private void StartAllTests()
        {
            var supported = Devices
                .Select(device => device.Category)
                .Where(IsTestSupported)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (supported.Length == 0)
            {
                StatusMessage = "Windows no detectó teclado, mouse, pantalla o audio que puedan probarse interactivamente.";
                return;
            }

            _testQueue = supported;
            _testIndex = 0;
            _testAll = true;
            StartCurrentTest();
        }

        private void StartDeviceTest(PeripheralDiagnostic? device)
        {
            if (device == null) return;
            if (!IsTestSupported(device.Category))
            {
                StatusMessage = $"«{device.Name}» está detectado, pero la prueba funcional solo cubre teclado, mouse, pantalla y salida de audio predeterminada. Su estado PnP permanece disponible en la tabla.";
                return;
            }

            _testQueue = [device.Category];
            _testIndex = 0;
            _testAll = false;
            StartCurrentTest();
            StatusMessage += " Windows no permite identificar qué dispositivo físico generó cada evento; desconecta los demás para aislar uno.";
        }

        private void StartCurrentTest()
        {
            if (_testIndex >= _testQueue.Length)
            {
                IsTesting = false;
                ActiveTestCategory = string.Empty;
                _testAll = false;
                StatusMessage = "Prueba secuencial terminada. Consulta los resultados y los estados PnP.";
                return;
            }

            ActiveTestCategory = _testQueue[_testIndex];
            IsTesting = true;
            ResetCounters();
            TestInstruction = ActiveTestCategory switch
            {
                "Teclado" => "Haz clic en el área de captura y pulsa las teclas que quieras comprobar. La aplicación solo muestra los nombres de las teclas; no guarda lo que escribas.",
                "Mouse" => "Mueve el puntero por el área y prueba los botones izquierdo, derecho, central y la rueda.",
                "Pantalla" => "Cambia los patrones de color. Marca «Correcto» si ves todos sin manchas, líneas o píxeles anómalos.",
                "Audio" => "Reproduce el tono de prueba por la salida de audio predeterminada. Marca «Correcto» si lo oyes.",
                _ => string.Empty
            };
            StatusMessage = _testAll
                ? $"Prueba {_testIndex + 1} de {_testQueue.Length}: {ActiveTestCategory}."
                : $"Prueba individual: {ActiveTestCategory}.";
        }

        private void CompleteTest(bool passed)
        {
            if (!IsTesting) return;
            string category = ActiveTestCategory;
            string detail = category switch
            {
                "Teclado" => $" · {ObservedKeys.Split(" · ", StringSplitOptions.RemoveEmptyEntries).Length} teclas observadas",
                "Mouse" => $" · clics I/D/C/adicionales: {LeftClicks}/{RightClicks}/{MiddleClicks}/{ExtraClicks}, rueda: {WheelSteps}, movimientos: {PointerMoves}",
                _ => string.Empty
            };
            TestResults.Insert(0, $"{DateTime.Now:HH:mm:ss} · {category}: {(passed ? "Correcto" : "Falló")}{detail}");
            while (TestResults.Count > 10)
                TestResults.RemoveAt(TestResults.Count - 1);

            _testIndex++;
            if (_testAll)
                StartCurrentTest();
            else
            {
                IsTesting = false;
                ActiveTestCategory = string.Empty;
                StatusMessage = $"Prueba de {category} {(passed ? "registrada como correcta." : "registrada con fallo.")}";
            }
        }

        private void AdvanceScreenPattern()
        {
            _screenPatternIndex = (_screenPatternIndex + 1) % ScreenPatterns.Length;
            OnPropertyChanged(nameof(ScreenPatternBrush));
        }

        private void ResetCounters()
        {
            LastKey = "Ninguna";
            ObservedKeys = string.Empty;
            LeftClicks = 0;
            RightClicks = 0;
            MiddleClicks = 0;
            ExtraClicks = 0;
            WheelSteps = 0;
            PointerMoves = 0;
            _screenPatternIndex = 0;
            OnPropertyChanged(nameof(ScreenPatternBrush));
        }

        private static bool IsTestSupported(string category) =>
            category.Equals("Teclado", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Mouse", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Pantalla", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Audio", StringComparison.OrdinalIgnoreCase);
    }
}
