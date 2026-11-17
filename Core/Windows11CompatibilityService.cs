using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Win32;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    [SupportedOSPlatform("windows")]
    public sealed class Windows11CompatibilityService : IWindows11CompatibilityService
    {
        public Task<IReadOnlyList<CompatibilityCheck>> CheckAsync() => Task.Run<IReadOnlyList<CompatibilityCheck>>(() =>
        {
            var checks = new List<CompatibilityCheck>
            {
                CheckMemory(),
                CheckSystemDrive(),
                CheckTpm(),
                CheckSecureBoot(),
                CheckProcessor()
            };
            return checks;
        });

        private static CompatibilityCheck CheckMemory()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
                using var results = searcher.Get();
                ulong totalBytes = 0;
                foreach (ManagementObject item in results)
                {
                    using (item)
                    {
                        if (ulong.TryParse(item["Capacity"]?.ToString(), out ulong capacity))
                            totalBytes += capacity;
                    }
                }

                if (totalBytes > 0)
                {
                    double memoryGb = totalBytes / 1024d / 1024d / 1024d;
                    return new CompatibilityCheck
                    {
                        Requirement = "Memoria RAM instalada",
                        Result = $"{memoryGb:F1} GB",
                        Status = memoryGb >= 4 ? "Cumple" : "No cumple",
                        Details = "Requisito mínimo: 4 GB."
                    };
                }
            }
            catch { }

            return Unknown("Memoria RAM", "No se pudo consultar la memoria instalada.");
        }

        private static CompatibilityCheck CheckSystemDrive()
        {
            try
            {
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                var drive = new DriveInfo(systemDrive);
                if (drive.IsReady)
                {
                    double capacityGb = drive.TotalSize / 1024d / 1024d / 1024d;
                    return new CompatibilityCheck
                    {
                        Requirement = "Almacenamiento del sistema",
                        Result = $"{drive.Name} · {capacityGb:F0} GB",
                        Status = capacityGb >= 64 ? "Cumple" : "Revisión manual",
                        Details = capacityGb >= 64
                            ? "La unidad del sistema tiene al menos 64 GB; el requisito de Microsoft se refiere al dispositivo de almacenamiento."
                            : "Esta es la capacidad de la partición del sistema, no necesariamente la del dispositivo físico; requisito: dispositivo de 64 GB."
                    };
                }
            }
            catch { }

            return Unknown("Almacenamiento del sistema", "No se pudo consultar el tamaño de la unidad.");
        }

        private static CompatibilityCheck CheckTpm()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\CIMV2\Security\MicrosoftTpm",
                    "SELECT SpecVersion, IsEnabled_InitialValue, IsActivated_InitialValue FROM Win32_Tpm");
                using var results = searcher.Get();
                foreach (ManagementObject item in results)
                {
                    using (item)
                    {
                        string versions = item["SpecVersion"]?.ToString() ?? string.Empty;
                        bool enabled = item["IsEnabled_InitialValue"] is bool isEnabled && isEnabled;
                        bool activated = item["IsActivated_InitialValue"] is bool isActivated && isActivated;
                        bool version2 = Array.Exists(versions.Split(','), version => version.Trim().StartsWith("2.0", StringComparison.OrdinalIgnoreCase));
                        bool meets = version2 && enabled && activated;
                        return new CompatibilityCheck
                        {
                            Requirement = "TPM",
                            Result = string.IsNullOrWhiteSpace(versions) ? "Detectado" : versions,
                            Status = meets ? "Cumple" : version2 ? "Atención" : "No cumple",
                            Details = meets
                                ? "TPM 2.0 está habilitado y activado."
                                : "Windows detectó el TPM, pero la versión 2.0, su activación o su estado no cumple el requisito."
                        };
                    }
                }

                return new CompatibilityCheck
                {
                    Requirement = "TPM",
                    Result = "No detectado",
                    Status = "No detectado",
                    Details = "No se encontró un dispositivo TPM mediante WMI."
                };
            }
            catch
            {
                return Unknown("TPM", "No se pudo consultar TPM por WMI; puede requerir una comprobación manual en UEFI.");
            }
        }

        private static CompatibilityCheck CheckSecureBoot()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                if (key?.GetValue("UEFISecureBootEnabled") is int state)
                {
                    return new CompatibilityCheck
                    {
                        Requirement = "UEFI / Arranque seguro",
                        Result = state == 1 ? "Arranque seguro habilitado" : "Arranque seguro deshabilitado",
                        Status = state == 1 ? "Cumple" : "Atención",
                        Details = state == 1
                            ? "El firmware informa que Secure Boot está habilitado."
                            : "Puede ser posible habilitarlo desde UEFI; no se modificó ninguna configuración."
                    };
                }
            }
            catch { }

            return Unknown("UEFI / Arranque seguro", "No se pudo determinar el estado de Secure Boot; compruébalo en la información del sistema o UEFI.");
        }

        private static CompatibilityCheck CheckProcessor()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                using var results = searcher.Get();
                foreach (ManagementObject item in results)
                {
                    using (item)
                    {
                        string name = item["Name"]?.ToString()?.Trim() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return new CompatibilityCheck
                            {
                                Requirement = "Procesador",
                                Result = name,
                                Status = "Revisión manual",
                                Details = "La compatibilidad depende del modelo exacto y de la lista oficial de CPU admitidas por Microsoft; esta comprobación no la certifica."
                            };
                        }
                    }
                }
            }
            catch { }

            return Unknown("Procesador", "No se pudo consultar el modelo del procesador.");
        }

        private static CompatibilityCheck Unknown(string requirement, string details) => new()
        {
            Requirement = requirement,
            Result = "No disponible",
            Status = "No verificable",
            Details = details
        };
    }
}
