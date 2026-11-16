using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels
{
    public class ProgramsViewModel : ViewModelBase
    {
        private readonly IWingetService _wingetService;
        private readonly IBloatwareService _bloatwareService;
        private readonly ILogService _logService;

        private bool _isBusy;
        private string _statusMessage = "Cargando programas...";
        private string _searchText = string.Empty;

        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        public ObservableCollection<BloatwareApp> InstalledApps { get; } = new();
        public ObservableCollection<WingetApp> UpdatableApps { get; } = new();

        public ICommand RefreshInstalledCommand { get; }
        public ICommand RefreshUpdatesCommand { get; }
        public ICommand UninstallAppCommand { get; }
        public ICommand UpdateAppCommand { get; }
        public ICommand UpdateAllCommand { get; }
        public ICommand OpenWindowsUpdateCommand { get; }
        public ICommand PauseWindowsUpdatesCommand { get; }
        public ICommand ResumeWindowsUpdatesCommand { get; }

        public ProgramsViewModel(IWingetService wingetService, IBloatwareService bloatwareService, ILogService logService)
        {
            _wingetService = wingetService;
            _bloatwareService = bloatwareService;
            _logService = logService;

            RefreshInstalledCommand = new AsyncRelayCommand(_ => LoadInstalledAppsAsync());
            RefreshUpdatesCommand = new AsyncRelayCommand(_ => LoadUpdatableAppsAsync());
            UninstallAppCommand = new AsyncRelayCommand(async p => await UninstallAppAsync(p));
            UpdateAppCommand = new AsyncRelayCommand(async p => await UpdateAppAsync(p));
            UpdateAllCommand = new AsyncRelayCommand(_ => UpdateAllAppsAsync());
            OpenWindowsUpdateCommand = new RelayCommand(_ => OpenWindowsUpdate());
            PauseWindowsUpdatesCommand = new RelayCommand(_ => PauseWindowsUpdates());
            ResumeWindowsUpdatesCommand = new RelayCommand(_ => ResumeWindowsUpdates());
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadInstalledAppsAsync();
            await LoadUpdatableAppsAsync();
        }

        public async Task LoadInstalledAppsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Cargando programas instalados...";

            try
            {
                var apps = await _bloatwareService.GetBloatwareAppsAsync();
                InstalledApps.Clear();
                foreach (var app in apps) InstalledApps.Add(app);

                StatusMessage = $"{InstalledApps.Count} programas instalados.";
                _logService.Info($"Installed apps: {InstalledApps.Count}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar programas: {ex.Message}";
                _logService.Error("Error al cargar programas instalados", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadUpdatableAppsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Buscando actualizaciones...";

            try
            {
                var apps = await _wingetService.GetUpdatableAppsAsync();
                UpdatableApps.Clear();
                foreach (var app in apps) UpdatableApps.Add(app);

                StatusMessage = $"{UpdatableApps.Count} actualizaciones disponibles.";
                _logService.Info($"Updatable apps: {UpdatableApps.Count}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al buscar actualizaciones: {ex.Message}";
                _logService.Error("Error al buscar actualizaciones", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UninstallAppAsync(object? parameter)
        {
            if (parameter is not BloatwareApp app) return;
            app.IsUninstalling = true;

            try
            {
                if (await _bloatwareService.UninstallBloatwareAppAsync(app))
                {
                    InstalledApps.Remove(app);
                    StatusMessage = $"{app.Name} desinstalado correctamente.";
                    _logService.Info($"Uninstalled: {app.Name}");
                }
            }
            finally
            {
                app.IsUninstalling = false;
            }
        }

        public async Task UpdateAppAsync(object? parameter)
        {
            if (parameter is not string id) return;
            var app = UpdatableApps.FirstOrDefault(x => x.Id == id);
            if (app == null) return;

            app.IsUpdating = true;
            try
            {
                await _wingetService.UpdateAppAsync(id, new Progress<(int, string)>(p =>
                {
                    app.UpdateProgress = p.Item1;
                    app.UpdateStatusMessage = p.Item2;
                }));
                StatusMessage = $"{app.Name} actualizado.";
                _logService.Info($"Updated: {app.Name}");
            }
            finally
            {
                app.IsUpdating = false;
            }
        }

        public async Task UpdateAllAppsAsync()
        {
            if (UpdatableApps.Count == 0)
            {
                MessageBox.Show("No hay actualizaciones pendientes.", "WPC-SutilBox", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Se van a actualizar {UpdatableApps.Count} aplicaciones. ¿Deseas continuar?",
                "Confirmar Actualización",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsBusy = true;
            StatusMessage = "Actualizando todas las aplicaciones...";
            try
            {
                await _wingetService.UpdateAllAppsAsync();
                await LoadUpdatableAppsAsync();
                StatusMessage = "Actualización completada.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenWindowsUpdate()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:windowsupdate",
                    UseShellExecute = true
                });
                StatusMessage = "Abriendo Windows Update...";
                _logService.Info("Abierto Windows Update");
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo abrir Windows Update: {ex.Message}";
                _logService.Error("Error al abrir Windows Update", ex);
            }
        }

        private void PauseWindowsUpdates()
        {
            try
            {
                var result = MessageBox.Show(
                    "Pausar las actualizaciones de Windows durante 7 días. ¿Desea continuar?",
                    "Pausar Actualizaciones",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"Add-WindowsAutoUpdateRestriction -Days 7\"",
                    UseShellExecute = true,
                    Verb = "runas"
                });
                StatusMessage = "Actualizaciones pausadas por 7 días.";
                _logService.Info("Windows Update pausado");
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo pausar: {ex.Message}";
                _logService.Error("Error al pausar Windows Update", ex);
            }
        }

        private void ResumeWindowsUpdates()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"Remove-WindowsAutoUpdateRestriction\"",
                    UseShellExecute = true,
                    Verb = "runas"
                });
                StatusMessage = "Actualizaciones reanudadas.";
                _logService.Info("Windows Update reanudado");
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo reanudar: {ex.Message}";
                _logService.Error("Error al reanudar Windows Update", ex);
            }
        }
    }
}
