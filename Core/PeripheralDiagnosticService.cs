using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    [SupportedOSPlatform("windows")]
    public sealed class PeripheralDiagnosticService : IPeripheralDiagnosticService
    {
        private readonly ILogService _logService;

        public PeripheralDiagnosticService(ILogService logService) => _logService = logService;

        public Task<IReadOnlyList<PeripheralDiagnostic>> GetDevicesAsync() =>
            Task.Run<IReadOnlyList<PeripheralDiagnostic>>(() =>
            {
                var devices = new List<PeripheralDiagnostic>();
                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        "SELECT Name, PNPClass, Manufacturer, Status, ConfigManagerErrorCode, PNPDeviceID " +
                        "FROM Win32_PnPEntity");
                    using var results = searcher.Get();

                    foreach (ManagementObject item in results)
                    {
                        using (item)
                        {
                            string name = item["Name"]?.ToString() ?? string.Empty;
                            string deviceClass = item["PNPClass"]?.ToString() ?? string.Empty;
                            string deviceId = item["PNPDeviceID"]?.ToString() ?? string.Empty;
                            string category = GetCategory(deviceClass, name, deviceId);
                            uint? errorCode = uint.TryParse(item["ConfigManagerErrorCode"]?.ToString(), out uint code)
                                ? code
                                : null;
                            string status = errorCode switch
                            {
                                0 => "Correcto",
                                10 => "No puede iniciarse (código 10)",
                                22 => "Deshabilitado (código 22)",
                                28 => "Controlador no instalado (código 28)",
                                43 => "Windows detuvo el dispositivo (código 43)",
                                null => item["Status"]?.ToString() ?? "Estado no disponible",
                                _ => $"Error de dispositivo (código {errorCode})"
                            };

                            devices.Add(new PeripheralDiagnostic
                            {
                                Category = category,
                                Name = string.IsNullOrWhiteSpace(name) ? "Dispositivo sin nombre" : name,
                                Manufacturer = item["Manufacturer"]?.ToString() ?? "No disponible",
                                Status = status,
                                DeviceId = deviceId
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error("No se pudieron consultar los dispositivos periféricos", ex);
                    throw;
                }

                return devices
                    .OrderBy(device => device.Category, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(device => device.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();
            });

        public Task<IReadOnlyList<UsbStorageDiagnostic>> GetUsbStorageDevicesAsync() =>
            Task.Run<IReadOnlyList<UsbStorageDiagnostic>>(() =>
            {
                var devices = new List<UsbStorageDiagnostic>();
                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        "SELECT Model, Size, Status, PNPDeviceID FROM Win32_DiskDrive " +
                        "WHERE InterfaceType = 'USB' OR PNPDeviceID LIKE 'USB%'");
                    using var results = searcher.Get();

                    foreach (ManagementObject item in results)
                    {
                        using (item)
                        {
                            string model = item["Model"]?.ToString() ?? "Dispositivo USB sin nombre";
                            string status = item["Status"]?.ToString() ?? "Estado no disponible";
                            string deviceId = item["PNPDeviceID"]?.ToString() ?? string.Empty;
                            string capacity = ulong.TryParse(item["Size"]?.ToString(), out ulong bytes)
                                ? $"{bytes / 1024d / 1024d / 1024d:F1} GB (nominal)"
                                : "No disponible";

                            devices.Add(new UsbStorageDiagnostic
                            {
                                Model = model,
                                Capacity = capacity,
                                Status = status,
                                DeviceId = deviceId
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error("No se pudieron consultar las unidades de almacenamiento USB", ex);
                    throw;
                }

                return devices.OrderBy(device => device.Model, StringComparer.CurrentCultureIgnoreCase).ToArray();
            });

        private static string GetCategory(string deviceClass, string name, string deviceId)
        {
            if (deviceClass.Equals("Keyboard", StringComparison.OrdinalIgnoreCase)) return "Teclado";
            if (deviceClass.Equals("Mouse", StringComparison.OrdinalIgnoreCase)) return "Mouse";
            if (deviceClass.Equals("Monitor", StringComparison.OrdinalIgnoreCase)) return "Pantalla";
            if (deviceClass.Equals("Camera", StringComparison.OrdinalIgnoreCase)
                || deviceClass.Equals("Image", StringComparison.OrdinalIgnoreCase)
                || name.Contains("camera", StringComparison.OrdinalIgnoreCase)
                || name.Contains("webcam", StringComparison.OrdinalIgnoreCase)) return "Cámara / imagen";
            if (deviceClass.Equals("Media", StringComparison.OrdinalIgnoreCase)
                || deviceClass.Equals("AudioEndpoint", StringComparison.OrdinalIgnoreCase)
                || deviceClass.Equals("Sound", StringComparison.OrdinalIgnoreCase)
                || name.Contains("audio", StringComparison.OrdinalIgnoreCase)
                || name.Contains("headset", StringComparison.OrdinalIgnoreCase)
                || name.Contains("speaker", StringComparison.OrdinalIgnoreCase)) return "Audio";
            if (deviceClass.Equals("USB", StringComparison.OrdinalIgnoreCase)
                || deviceId.Contains("USB\\", StringComparison.OrdinalIgnoreCase)) return "USB";
            return string.IsNullOrWhiteSpace(deviceClass) ? "Otros" : $"Otros · {deviceClass}";
        }
    }
}
