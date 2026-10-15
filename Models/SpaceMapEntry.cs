using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wpc_SutilBox.Models;

public sealed class SpaceMapEntry : INotifyPropertyChanged
{
    private bool _isSelected;
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public long SizeBytes { get; internal set; }
    public DateTime? LastModified { get; init; }
    public bool IsDirectory { get; init; }
    public string FormattedSize => FormatBytes(SizeBytes);
    public string CategoryColor => Category switch
    {
        "Video" => "#8B5CF6",
        "Audio" => "#0EA5E9",
        "Imagen" => "#10B981",
        "Ejecutable" => "#EF4444",
        "Comprimido" => "#3B82F6",
        "Imagen de disco" => "#F97316",
        "Documento" => "#14B8A6",
        "Temporal" => "#F59E0B",
        "Código" => "#6366F1",
        "Registro" => "#78716C",
        _ => IsDirectory ? "#334155" : "#475569"
    };
    public ObservableCollection<SpaceMapEntry> Children { get; } = new();
    public bool IsSelected { get => _isSelected; set { if (_isSelected == value) return; _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    public static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = Math.Max(0, bytes);
        var index = 0;
        while (value >= 1024 && index < units.Length - 1) { value /= 1024; index++; }
        return $"{value:0.##} {units[index]}";
    }
}
