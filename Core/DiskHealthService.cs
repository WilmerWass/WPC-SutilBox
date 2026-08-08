using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

#nullable enable

namespace Wpc_SutilBox.Core
{
    public class DiskHealthService : IDiskHealthService
    {
        public async Task<IEnumerable<DiskHealthInfo>> GetDiskHealthAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<DiskHealthInfo>();
                var smartEntries = GetSmartEntries(); // Obtiene SMART sin lanzar excepciones

                try
                {
                    using var drives = new System.Management.ManagementObjectSearcher("SELECT DeviceID, Model, SerialNumber, PNPDeviceID, Index, Size FROM Win32_DiskDrive");
                    foreach (var d in drives.Get())
                    {
                        if (d == null) continue;

                        try
                        {
                            var id = d["DeviceID"]?.ToString() ?? "";
                            var model = d["Model"]?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(model)) model = "Unidad de Almacenamiento";

                            var serial = d["SerialNumber"]?.ToString()?.Trim() ?? "N/D";
                            var pnpDeviceId = d["PNPDeviceID"]?.ToString();
                            int? index = d["Index"] != null ? Convert.ToInt32(d["Index"]) : (int?)null;
                            long? sizeBytes = d["Size"] != null ? Convert.ToInt64(d["Size"]) : (long?)null;

                            var status = GetSmartStatus(smartEntries, index, pnpDeviceId, model);

                            list.Add(new DiskHealthInfo
                            {
                                DeviceId = id,
                                Model = model,
                                Serial = serial,
                                Capacity = sizeBytes.HasValue ? FormatBytes(sizeBytes.Value) : "N/D",
                                SmartOk = status.HasValue && status.Value,
                                SmartStatusKnown = status.HasValue,
                                SmartStatus = status.HasValue ? (status.Value ? "OK" : "FALLA") : "Sin datos SMART",
                                Temperature = GetDiskTemperature(index, id),
                                PnpDeviceId = pnpDeviceId,
                                PhysicalDiskIndex = index
                            });
                        }
                        catch
                        {
                            // Error procesando un disco individual, continuamos con el siguiente
                        }
                    }
                }
                catch
                {
                    // Fallback general si Win32_DiskDrive falla por completo
                }

                // Fallback secundario: Si Win32_DiskDrive no devolvió discos, intentamos vía MSFT_PhysicalDisk (NVMe/Storage API)
                if (list.Count == 0)
                {
                    try
                    {
                        using var searcher = new System.Management.ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", "SELECT FriendlyName, SerialNumber, Size, HealthStatus, DeviceId FROM MSFT_PhysicalDisk");
                        foreach (var mo in searcher.Get())
                        {
                            var model = mo["FriendlyName"]?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(model)) model = "SSD NVMe / SATA";

                            var serial = mo["SerialNumber"]?.ToString()?.Trim() ?? "N/D";
                            var deviceId = mo["DeviceId"]?.ToString() ?? "0";
                            long? sizeBytes = mo["Size"] != null ? Convert.ToInt64(mo["Size"]) : (long?)null;
                            int health = mo["HealthStatus"] != null ? Convert.ToInt32(mo["HealthStatus"]) : -1;

                            bool? isOk = health == 0 ? true : (health == 1 || health == 2 ? false : (bool?)null);

                            int? physicalIndex = int.TryParse(deviceId, out int parsedIdx) ? parsedIdx : (int?)null;

                            list.Add(new DiskHealthInfo
                            {
                                DeviceId = deviceId,
                                Model = model,
                                Serial = serial,
                                Capacity = sizeBytes.HasValue ? FormatBytes(sizeBytes.Value) : "N/D",
                                SmartOk = isOk.HasValue && isOk.Value,
                                SmartStatusKnown = isOk.HasValue,
                                SmartStatus = isOk.HasValue ? (isOk.Value ? "OK" : "REVISAR") : "Sin datos SMART",
                                Temperature = GetDiskTemperature(physicalIndex, deviceId),
                                PnpDeviceId = null,
                                PhysicalDiskIndex = physicalIndex
                            });
                        }
                    }
                    catch
                    {
                        // Captura silenciosa si el namespace de Storage tampoco responde
                    }
                }

                return list;
            });
        }

        private static int GetDiskTemperature(int? diskIndex, string? deviceId = null)
        {
            // Intento 1: API de Almacenamiento de Windows (NVMe, M.2 y SSDs modernos)
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    @"root\Microsoft\Windows\Storage",
                    "SELECT DeviceId, Temperature FROM MSFT_StorageReliabilityCounter");

                foreach (var mo in searcher.Get())
                {
                    if (mo["Temperature"] != null)
                    {
                        var devId = mo["DeviceId"]?.ToString();
                        
                        // Validar coincidencia por ID de dispositivo o índice
                        if ((diskIndex.HasValue && devId == diskIndex.Value.ToString()) ||
                            (!string.IsNullOrEmpty(deviceId) && deviceId.Contains(devId ?? "")))
                        {
                            int temp = Convert.ToInt32(mo["Temperature"]);
                            if (temp > 0 && temp < 120) return temp; // Rango válido en Celsius
                        }
                    }
                }
            }
            catch
            {
                // Fallback silencioso si el contador no está soportado
            }

            // Intento 2: MSAcpi_ThermalZoneTemperature (ACPI Legacy / SATA)
            if (diskIndex.HasValue)
            {
                try
                {
                    string query = $"SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature WHERE InstanceName LIKE '%PhysicalDisk{diskIndex.Value}%'";
                    using var searcher = new System.Management.ManagementObjectSearcher(@"root\WMI", query);

                    foreach (var mo in searcher.Get())
                    {
                        if (mo["CurrentTemperature"] != null)
                        {
                            int kelvinTenths = Convert.ToInt32(mo["CurrentTemperature"]);
                            int celsius = (int)((kelvinTenths / 10.0) - 273.15);
                            if (celsius > 0 && celsius < 120) return celsius;
                        }
                    }
                }
                catch
                {
                    // Ignorar error
                }

                // Intento 3: Win32_TemperatureProbe (Motherboards / Sensores CIMV2)
                try
                {
                    string query = $"SELECT CurrentReading FROM Win32_TemperatureProbe WHERE InstanceName LIKE '%Disk{diskIndex.Value}%'";
                    using var searcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", query);
                    foreach (var mo in searcher.Get())
                    {
                        if (mo["CurrentReading"] != null)
                        {
                            int temp = Convert.ToInt32(mo["CurrentReading"]);
                            if (temp > 0 && temp < 120) return temp;
                        }
                    }
                }
                catch
                {
                    // Ignorar error
                }
            }

            return 0; // Se formateará en la interfaz como "--" si la unidad no expone sensor
        }

        private static List<(string InstanceName, bool? SmartOk)> GetSmartEntries()
        {
            var list = new List<(string InstanceName, bool? SmartOk)>();

            // Intento 1: MSStorageDriver_FailurePredictStatus (SATA/ATAPI clásico)
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(@"root\WMI", "SELECT PredictFailure, InstanceName FROM MSStorageDriver_FailurePredictStatus");
                foreach (var mo in searcher.Get())
                {
                    var instance = mo["InstanceName"]?.ToString() ?? "";
                    bool? ok = null;
                    if (mo["PredictFailure"] != null)
                    {
                        ok = Convert.ToInt32(mo["PredictFailure"]) == 0;
                    }
                    if (!string.IsNullOrEmpty(instance)) list.Add((instance, ok));
                }
            }
            catch
            {
                // Captura silenciosa
            }

            // Intento 2: MSStorageDriver_FailurePredictData
            if (list.Count == 0)
            {
                try
                {
                    using var searcher = new System.Management.ManagementObjectSearcher(@"root\WMI", "SELECT InstanceName, PredictFailure FROM MSStorageDriver_FailurePredictData");
                    foreach (var mo in searcher.Get())
                    {
                        var instance = mo["InstanceName"]?.ToString() ?? "";
                        bool? ok = null;
                        if (mo["PredictFailure"] != null)
                        {
                            ok = Convert.ToInt32(mo["PredictFailure"]) == 0;
                        }
                        if (!string.IsNullOrEmpty(instance)) list.Add((instance, ok));
                    }
                }
                catch
                {
                    // Captura silenciosa
                }
            }

            // Intento 3: MSFT_PhysicalDisk (NVMe y SSDs en Windows 10/11)
            if (list.Count == 0)
            {
                try
                {
                    using var searcher = new System.Management.ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", "SELECT FriendlyName, HealthStatus FROM MSFT_PhysicalDisk");
                    foreach (var mo in searcher.Get())
                    {
                        var instance = mo["FriendlyName"]?.ToString() ?? "";
                        bool? ok = null;
                        if (mo["HealthStatus"] != null)
                        {
                            int health = Convert.ToInt32(mo["HealthStatus"]);
                            ok = (health == 0); // 0 = Healthy
                        }
                        if (!string.IsNullOrEmpty(instance)) list.Add((instance, ok));
                    }
                }
                catch
                {
                    // Captura silenciosa
                }
            }

            return list;
        }

        private static bool? GetSmartStatus(List<(string InstanceName, bool? SmartOk)> entries, int? diskIndex, string? pnpDeviceId, string model)
        {
            if (entries.Count == 0) return null;

            try
            {
                var entry = entries.FirstOrDefault(e =>
                {
                    // Coincidencia por índice de disco
                    if (diskIndex.HasValue && e.InstanceName.EndsWith($"_{diskIndex.Value}", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Coincidencia por PNPDeviceID
                    if (!string.IsNullOrWhiteSpace(pnpDeviceId))
                    {
                        string pnpNorm = NormalizeForMatch(pnpDeviceId);
                        if (!string.IsNullOrEmpty(pnpNorm) && NormalizeForMatch(e.InstanceName).Contains(pnpNorm, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    // Coincidencia por Nombre/Modelo
                    if (!string.IsNullOrWhiteSpace(model))
                    {
                        string modelNorm = NormalizeForMatch(model);
                        if (!string.IsNullOrEmpty(modelNorm) && NormalizeForMatch(e.InstanceName).Contains(modelNorm, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                });

                return entry.SmartOk;
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeForMatch(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return new string(s.Where(char.IsLetterOrDigit).ToArray());
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

