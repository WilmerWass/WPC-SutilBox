using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels;

public sealed class SpaceMapViewModel : ViewModelBase
{
    private readonly ISpaceMapService _service;
    private readonly IDialogService _dialogs;
    private CancellationTokenSource? _scanCancellation;
    private string _selectedFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private bool _isScanning;
    private bool _isDeleting;
    private string _status = "Elige una carpeta y analiza su contenido.";

    public SpaceMapViewModel(ISpaceMapService service, IDialogService dialogs) { _service = service; _dialogs = dialogs; }
    public ObservableCollection<SpaceMapEntry> Roots { get; } = new();
    public ObservableCollection<SpaceMapEntry> Files { get; } = new();
    public string SelectedFolder { get => _selectedFolder; set => SetProperty(ref _selectedFolder, value); }
    public bool IsScanning { get => _isScanning; private set => SetProperty(ref _isScanning, value); }
    public bool IsDeleting { get => _isDeleting; private set => SetProperty(ref _isDeleting, value); }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public long TotalBytes => Roots.Sum(root => root.SizeBytes);
    public int FileCount => Files.Count;
    public ICommand BackCommand { get; set; } = new RelayCommand(() => { });
    public ICommand BrowseCommand => new RelayCommand(_ => Browse());
    public ICommand AnalyzeCommand => new AsyncRelayCommand(_ => AnalyzeAsync());
    public ICommand CancelCommand => new RelayCommand(_ => _scanCancellation?.Cancel());
    public ICommand OpenLocationCommand => new RelayCommand(OpenLocation);
    public ICommand DeleteSelectedCommand => new AsyncRelayCommand(_ => DeleteSelectedAsync());

    private void Browse()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = SelectedFolder, Description = "Selecciona una carpeta para analizar" };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) SelectedFolder = dialog.SelectedPath;
    }

    private async Task AnalyzeAsync()
    {
        if (IsScanning || IsDeleting) return;
        var roots = new[] { SelectedFolder, System.IO.Path.GetTempPath(), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp") }.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        _scanCancellation = new CancellationTokenSource();
        IsScanning = true;
        Status = "Analizando carpetas…";
        try
        {
            var result = await _service.AnalyzeAsync(roots, _scanCancellation.Token);
            Roots.Clear(); Files.Clear();
            foreach (var root in result)
            {
                Roots.Add(root);
                AddFiles(root);
            }
            OnPropertyChanged(nameof(TotalBytes)); OnPropertyChanged(nameof(FileCount));
            Status = $"{FileCount:N0} archivos · {SpaceMapEntry.FormatBytes(TotalBytes)} de tamaño lógico estimado. Las carpetas no accesibles pueden omitirse.";
        }
        catch (OperationCanceledException) { Status = "Análisis cancelado."; }
        catch (Exception ex) { Status = $"Error al analizar: {ex.Message}"; }
        finally { IsScanning = false; _scanCancellation.Dispose(); _scanCancellation = null; }
    }

    private static System.Collections.Generic.IEnumerable<string> EnumerateUserTempRoots()
    {
        var users = System.IO.Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "Users");
        string[] profiles;
        try { profiles = System.IO.Directory.EnumerateDirectories(users).ToArray(); }
        catch (System.IO.IOException) { yield break; }
        catch (System.UnauthorizedAccessException) { yield break; }
        foreach (var profile in profiles)
            yield return System.IO.Path.Combine(profile, "AppData", "Local", "Temp");
    }

    private void AddFiles(SpaceMapEntry entry)
    {
        if (!entry.IsDirectory) { Files.Add(entry); return; }
        foreach (var child in entry.Children) AddFiles(child);
    }

    private static void OpenLocation(object? parameter)
    {
        if (parameter is not SpaceMapEntry entry) return;
        try { Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{entry.Path}\"", UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show($"No se pudo abrir la ubicación: {ex.Message}", "Mapa de espacio", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private async Task DeleteSelectedAsync()
    {
        if (IsScanning || IsDeleting) return;
        var selected = Files.Where(file => file.IsSelected).ToArray();
        if (selected.Length == 0) { Status = "Selecciona archivos para continuar."; return; }
        long estimate = selected.Sum(file => Math.Max(0, file.SizeBytes));
        if (!await _dialogs.ShowConfirmation($"Revisión 1 de 3: has seleccionado {selected.Length} archivos ({SpaceMapEntry.FormatBytes(estimate)} estimados). ¿Quieres revisar el borrado?", "Revisar selección")) return;
        if (!await _dialogs.ShowConfirmation("Revisión 2 de 3: el borrado es permanente y los archivos no pasan por la Papelera. Solo se permite borrar archivos de las ubicaciones temporales autorizadas; las demás selecciones fallarán de forma segura. ¿Deseas continuar?", "Advertencia de borrado permanente")) return;
        if (!await _dialogs.ShowConfirmation($"Confirmación 3 de 3: se intentará borrar {selected.Length} archivos seleccionados. Cada ruta se revalidará justo antes. ¿Confirmas?", "Confirmación final")) return;
        IsDeleting = true; Status = "Validando y borrando archivos seleccionados…";
        try
        {
            var temporaryRoots = new[] { System.IO.Path.GetTempPath(), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp") }
                .Concat(EnumerateUserTempRoots())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var result = await _service.DeleteSelectedAsync(selected, temporaryRoots);
            foreach (var file in selected.Where(file => !System.IO.File.Exists(file.Path))) Files.Remove(file);
            OnPropertyChanged(nameof(FileCount));
            Status = $"Eliminados: {result.Deleted}; omitidos: {result.Skipped}; fallidos: {result.Failed}; tamaño lógico de los eliminados: {SpaceMapEntry.FormatBytes(result.EstimatedBytes)}.";
        }
        catch (Exception ex) { Status = $"No se completó el borrado: {ex.Message}"; }
        finally { IsDeleting = false; }
    }
}
