using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Win32;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public class PrivacyService : IPrivacyService
    {
        private readonly ILogService _log;
        private readonly IDialogService _dialogService;

        public PrivacyService(ILogService log, IDialogService dialogService)
        {
            _log = log;
            _dialogService = dialogService;
        }

        public async Task<IEnumerable<PrivacySetting>> GetPrivacySettingsAsync()
        {
            return await Task.Run(() =>
            {
                var settings = new List<PrivacySetting>();

                // Telemetría y diagnóstico
                settings.Add(GetTelemetrySetting());
                settings.Add(GetDiagnosticDataSetting());
                settings.Add(GetAdvertisingIdSetting());

                // Servicios de ubicación
                settings.Add(GetLocationServiceSetting());

                // Actividad reciente
                settings.Add(GetActivityHistorySetting());
                settings.Add(GetRecentFilesSetting());

                // Búsqueda Windows
                settings.Add(GetCloudSearchSetting());
                settings.Add(GetSearchHistorySetting());

                // Cortana / Asistente
                settings.Add(GetCortanaSetting());

                // Permisos de apps
                settings.Add(GetCameraAppAccessSetting());
                settings.Add(GetMicrophoneAppAccessSetting());
                settings.Add(GetBluetoothAppAccessSetting());

                return settings;
            });
        }

        public async Task<bool> UpdatePrivacySettingAsync(PrivacySetting setting, bool newValue)
        {
            return await Task.Run(async () =>
            {
                if (setting == null) return false;

                try
                {
                    _log.Info($"Updating privacy setting '{setting.Name}' to '{newValue}'");

                    // Una advertencia para configuraciones críticas
                    if (setting.Type == PrivacySettingType.Telemetry && newValue == true)
                    {
                        bool confirm = await _dialogService.ShowConfirmation($"Habilitar la telemetría podría enviar datos de uso a Microsoft. ¿Está seguro?", "Confirmar");
                        if (!confirm) return false;
                    }
                    if (setting.Type == PrivacySettingType.AdvertisingId && newValue == true)
                    {
                        bool confirm = await _dialogService.ShowConfirmation($"Habilitar el ID de publicidad permitirá que las aplicaciones utilicen su actividad para anuncios personalizados. ¿Está seguro?", "Confirmar");
                        if (!confirm) return false;
                    }

                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(setting.RegistryPath, true))
                    {
                        if (key != null)
                        {
                            key.SetValue(setting.RegistryValueName, newValue ? 1 : 0, RegistryValueKind.DWord);
                            setting.CurrentValue = newValue; // Actualizar modelo para la interfaz de usuario
                            _log.Info($"Privacy setting '{setting.Name}' updated successfully.");
                            return true;
                        }
                    }
                    _log.Warn($"Could not open/create registry key for setting '{setting.Name}' at '{setting.RegistryPath}'");
                    return false;
                }
                catch (Exception ex)
                {
                    _log.Error($"Error updating privacy setting '{setting.Name}'", ex);
                    return false;
                }
            });
        }

        private PrivacySetting GetTelemetrySetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy";
            string valueName = "AllowTelemetry"; // Declare valueName here
            int currentValue = 1; // Por defecto activado

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Telemetry setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Telemetría de Windows",
                Description = "Controla la cantidad de datos de diagnóstico y uso enviados a Microsoft.",
                CurrentValue = currentValue == 1,
                RecommendedValue = false, // Recomendar deshabilitar por privacidad
                Type = PrivacySettingType.Telemetry,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetAdvertisingIdSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
            string valueName = "Enabled";
            int currentValue = 1;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Advertising ID setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "ID de Publicidad Personalizada",
                Description = "Permite que las aplicaciones usen su actividad para mostrarle anuncios personalizados.",
                CurrentValue = currentValue == 1,
                RecommendedValue = false,
                Type = PrivacySettingType.AdvertisingId,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetDiagnosticDataSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection";
            string valueName = "AllowTelemetry";
            bool currentValue = true;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Diagnostic Data setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Datos de diagnóstico",
                Description = "Envía datos de diagnóstico a Microsoft para mejorar Windows.",
                CurrentValue = currentValue,
                RecommendedValue = false,
                Type = PrivacySettingType.DiagnosticData,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetLocationServiceSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location";
            string valueName = "Value";
            string currentValue = "Allow";

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null)
                        {
                            currentValue = val.ToString() ?? "Allow";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Location setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Servicio de ubicación",
                Description = "Permite que las aplicaciones accedan a tu ubicación.",
                CurrentValue = currentValue == "Allow",
                RecommendedValue = false,
                Type = PrivacySettingType.LocationService,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetActivityHistorySetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            string valueName = "ShowActivityFeed";
            int currentValue = 1;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Activity History setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Historial de actividad",
                Description = "Permite que Windows recopile y muestre tu historial de actividad.",
                CurrentValue = currentValue == 1,
                RecommendedValue = false,
                Type = PrivacySettingType.ActivityHistory,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetRecentFilesSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            string valueName = "LaunchTo";
            int currentValue = 1; // 1 = This PC, 2 = Quick Access (shows recent files)

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Recent Files setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Mostrar archivos recientes",
                Description = "Muestra archivos y carpetas recientes en el Explorador de archivos.",
                CurrentValue = currentValue == 2,
                RecommendedValue = false,
                Type = PrivacySettingType.Other,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetCloudSearchSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search";
            string valueName = "CloudSearch";
            int currentValue = 1;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Cloud Search setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Búsqueda en la nube",
                Description = "Permite que la búsqueda de Windows use contenido de la nube.",
                CurrentValue = currentValue == 1,
                RecommendedValue = false,
                Type = PrivacySettingType.Other,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetSearchHistorySetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings";
            string valueName = "IsDeviceSearchHistoryEnabled";
            int currentValue = 1;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Search History setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Historial de búsqueda",
                Description = "Guarda el historial de búsqueda en el dispositivo.",
                CurrentValue = currentValue == 1,
                RecommendedValue = false,
                Type = PrivacySettingType.Other,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetCortanaSetting()
        {
            string path = @"SOFTWARE\Microsoft\Personalization\Settings";
            string valueName = "AcceptedPrivacyPolicy";
            int currentValue = 1;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null && int.TryParse(val.ToString(), out int result))
                        {
                            currentValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Cortana setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Cortana / Asistente",
                Description = "Permite que Cortana recopile datos de uso y preferencias.",
                CurrentValue = currentValue == 1,
                RecommendedValue = false,
                Type = PrivacySettingType.Cortana,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetCameraAppAccessSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam";
            string valueName = "Value";
            string currentValue = "Allow";

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null)
                        {
                            currentValue = val.ToString() ?? "Allow";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Camera setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Acceso a cámara de apps",
                Description = "Permite que las aplicaciones de escritorio accedan a la cámara.",
                CurrentValue = currentValue == "Allow",
                RecommendedValue = false,
                Type = PrivacySettingType.AppPermissions,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetMicrophoneAppAccessSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone";
            string valueName = "Value";
            string currentValue = "Allow";

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null)
                        {
                            currentValue = val.ToString() ?? "Allow";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Microphone setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Acceso a micrófono de apps",
                Description = "Permite que las aplicaciones de escritorio accedan al micrófono.",
                CurrentValue = currentValue == "Allow",
                RecommendedValue = false,
                Type = PrivacySettingType.AppPermissions,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }

        private PrivacySetting GetBluetoothAppAccessSetting()
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\bluetooth";
            string valueName = "Value";
            string currentValue = "Allow";

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val != null)
                        {
                            currentValue = val.ToString() ?? "Allow";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not read Bluetooth setting: {ex.Message}");
            }

            return new PrivacySetting
            {
                Name = "Acceso a Bluetooth de apps",
                Description = "Permite que las aplicaciones de escritorio accedan a dispositivos Bluetooth.",
                CurrentValue = currentValue == "Allow",
                RecommendedValue = false,
                Type = PrivacySettingType.AppPermissions,
                RegistryPath = path,
                RegistryValueName = valueName
            };
        }
    }
}

