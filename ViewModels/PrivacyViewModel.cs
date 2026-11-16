using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class PrivacyViewModel : ViewModelBase
    {
        private readonly IPrivacyService _privacyService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Cargando configuraciones de privacidad...";

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        public ObservableCollection<PrivacySetting> PrivacySettings { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand ToggleSettingCommand { get; }
        public ICommand ApplyAllCommand { get; }

        public PrivacyViewModel(IPrivacyService privacyService, ILogService logService)
        {
            _privacyService = privacyService;
            _logService = logService;

            RefreshCommand = new AsyncRelayCommand(_ => LoadPrivacySettingsAsync());
            ToggleSettingCommand = new AsyncRelayCommand(async p => await ToggleSettingAsync(p));
            ApplyAllCommand = new AsyncRelayCommand(_ => ApplyAllAsync());
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadPrivacySettingsAsync();
        }

        public async Task LoadPrivacySettingsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Cargando configuraciones de privacidad...";

            try
            {
                var settings = await _privacyService.GetPrivacySettingsAsync();
                PrivacySettings.Clear();
                foreach (var setting in settings) PrivacySettings.Add(setting);

                StatusMessage = $"{PrivacySettings.Count} configuraciones de privacidad.";
                _logService.Info($"Privacy settings: {PrivacySettings.Count}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar configuraciones: {ex.Message}";
                _logService.Error("Error al cargar configuraciones de privacidad", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ToggleSettingAsync(object? parameter)
        {
            if (parameter is not PrivacySetting setting) return;

            try
            {
                var newValue = !setting.CurrentValue;
                var result = await _privacyService.UpdatePrivacySettingAsync(setting, newValue);
                if (result)
                {
                    setting.CurrentValue = newValue;
                    StatusMessage = $"{setting.Name}: {(newValue ? "activado" : "desactivado")}.";
                    _logService.Info($"Privacy setting toggled: {setting.Name}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al modificar: {ex.Message}";
                _logService.Error($"Error al modificar {setting.Name}", ex);
            }
        }

        public async Task ApplyAllAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Aplicando configuraciones recomendadas de privacidad...";

            try
            {
                int success = 0;
                int failed = 0;

                foreach (var setting in PrivacySettings)
                {
                    try
                    {
                        var result = await _privacyService.UpdatePrivacySettingAsync(setting, setting.RecommendedValue);
                        if (result)
                        {
                            setting.CurrentValue = setting.RecommendedValue;
                            success++;
                        }
                        else
                        {
                            failed++;
                        }
                    }
                    catch
                    {
                        failed++;
                    }
                }

                StatusMessage = $"Aplicado: {success} exitosos, {failed} fallidos.";
                _logService.Info($"Privacy apply all: {success} success, {failed} failed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al aplicar: {ex.Message}";
                _logService.Error("Error al aplicar configuraciones de privacidad", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
