using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.ViewModels;

public sealed class StorageViewModel : ViewModelBase
{
    private readonly IStorageScanService _scanService;
    private CancellationTokenSource? _scanCancellation;
    private StorageVolumeInfo? _selectedVolume;
    private string _selectedPath = string.Empty;
    private bool _isScanning;
    private string _statusMessage = "Selecciona una unidad o carpeta y comienza el análisis.";
    private string _searchText = string.Empty;
    private long _filesScanned;
    private long _directoriesScanned;
    private long _totalBytes;
    private bool _isPartial;

    public StorageViewModel(IStorageScanService scanService)
    {
        _scanService = scanService;
        Volumes = new ObservableCollection<StorageVolumeInfo>(_scanService.GetVolumes());
        if (Volumes.Count > 0)
        {
            SelectedVolume = Volumes[0];
            SelectedPath = SelectedVolume.Name;
        }
        Items = new ObservableCollection<StorageScanItem>();
        Errors = new ObservableCollection<StorageScanError>();
        ScanCommand = new AsyncRelayCommand(ScanAsync, () => !IsScanning && !string.IsNullOrWhiteSpace(SelectedPath));
        CancelCommand = new RelayCommand(_ => _scanCancellation?.Cancel(), _ => IsScanning);
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        RefreshVolumesCommand = new RelayCommand(_ => RefreshVolumes(), _ => !IsScanning);
    }

    public ObservableCollection<StorageVolumeInfo> Volumes { get; }
    public ObservableCollection<StorageScanItem> Items { get; }
    public ObservableCollection<StorageScanError> Errors { get; }
    public ICommand ScanCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseFolderCommand { get; }
    public ICommand RefreshVolumesCommand { get; }

    public StorageVolumeInfo? SelectedVolume
    {
        get => _selectedVolume;
        set
        {
            if (SetProperty(ref _selectedVolume, value) && value is not null)
                SelectedPath = value.Name;
        }
    }

    public string SelectedPath { get => _selectedPath; set => SetProperty(ref _selectedPath, value); }
    public bool IsScanning { get => _isScanning; private set { if (SetProperty(ref _isScanning, value)) { CommandManager.InvalidateRequerySuggested(); OnPropertyChanged(nameof(IsIdle)); } } }
    public bool IsIdle => !IsScanning;
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) OnPropertyChanged(nameof(FilteredItems)); } }
    public long FilesScanned { get => _filesScanned; private set => SetProperty(ref _filesScanned, value); }
    public long DirectoriesScanned { get => _directoriesScanned; private set => SetProperty(ref _directoriesScanned, value); }
    public long TotalBytes { get => _totalBytes; private set { if (SetProperty(ref _totalBytes, value)) OnPropertyChanged(nameof(FormattedTotalBytes)); } }
    public string FormattedTotalBytes => FormatSize(TotalBytes);
    public bool IsPartial { get => _isPartial; private set => SetProperty(ref _isPartial, value); }
    public System.Collections.Generic.IEnumerable<StorageScanItem> FilteredItems => string.IsNullOrWhiteSpace(SearchText)
        ? Items
        : Items.Where(item => item.Path.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    public void RefreshVolumes()
    {
        var selectedName = SelectedVolume?.Name;
        Volumes.Clear();
        foreach (var volume in _scanService.GetVolumes()) Volumes.Add(volume);
        SelectedVolume = Volumes.FirstOrDefault(v => v.Name == selectedName) ?? Volumes.FirstOrDefault();
    }

    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog { Title = "Seleccionar carpeta para analizar", Multiselect = false };
        if (dialog.ShowDialog() == true) SelectedPath = dialog.FolderName;
    }

    private async Task ScanAsync()
    {
        _scanCancellation?.Dispose();
        _scanCancellation = new CancellationTokenSource();
        var token = _scanCancellation.Token;
        IsScanning = true;
        IsPartial = false;
        FilesScanned = 0;
        DirectoriesScanned = 0;
        TotalBytes = 0;
        Items.Clear();
        Errors.Clear();
        StatusMessage = "Analizando… El escaneo no modifica archivos.";

        var progress = new Progress<StorageScanProgress>(value =>
        {
            FilesScanned = value.FilesScanned;
            DirectoriesScanned = value.DirectoriesScanned;
            StatusMessage = $"Analizando… {value.FilesScanned:N0} archivos y {value.DirectoriesScanned:N0} carpetas";
        });

        try
        {
            var result = await _scanService.ScanAsync(SelectedPath, progress, token);
            foreach (var item in result.Items) Items.Add(item);
            foreach (var error in result.Errors) Errors.Add(error);
            FilesScanned = result.FilesScanned;
            DirectoriesScanned = result.DirectoriesScanned;
            TotalBytes = result.TotalFileBytes;
            IsPartial = result.IsPartial;
            StatusMessage = result.IsCancelled
                ? $"Análisis cancelado: {result.FilesScanned:N0} archivos, {result.Errors.Count:N0} incidencias."
                : result.IsPartial
                    ? $"Análisis parcial: {result.FilesScanned:N0} archivos, {result.Errors.Count:N0} incidencias de acceso/lectura."
                    : $"Análisis completo: {result.FilesScanned:N0} archivos en {result.DirectoriesScanned:N0} carpetas.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Análisis cancelado.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo analizar la ruta: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            _scanCancellation?.Dispose();
            _scanCancellation = null;
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1) { size /= 1024; unit++; }
        return $"{size:0.##} {units[unit]}";
    }
}
