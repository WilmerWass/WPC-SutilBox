using System;
using System.Collections.Generic;
using System.IO;

namespace Wpc_SutilBox.Models;

public sealed record StorageVolumeInfo(string Name, string Label, string FileSystem, long TotalBytes, long AvailableBytes)
{
    public long UsedBytes => Math.Max(0, TotalBytes - AvailableBytes);
}

public sealed record StorageScanItem(string Path, bool IsDirectory, long SizeBytes, DateTime? LastWriteTimeUtc)
{
    public string Name => System.IO.Path.GetFileName(System.IO.Path.TrimEndingDirectorySeparator(Path));
    public string Extension => IsDirectory ? string.Empty : System.IO.Path.GetExtension(Path);
}

public sealed record StorageScanProgress(long FilesScanned, long DirectoriesScanned, string CurrentPath)
{
    public long ItemsScanned => FilesScanned + DirectoriesScanned;
}

public sealed record StorageScanError(string Path, string Message);

public sealed record StorageScanResult(
    string RootPath,
    IReadOnlyList<StorageScanItem> Items,
    IReadOnlyList<StorageScanError> Errors,
    long FilesScanned,
    long DirectoriesScanned,
    long TotalFileBytes,
    bool IsCancelled)
{
    public bool IsPartial => Errors.Count > 0;
    public bool IsComplete => !IsCancelled && Errors.Count == 0;
}
