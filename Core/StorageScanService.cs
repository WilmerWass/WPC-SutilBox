using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core;

public sealed class StorageScanService : IStorageScanService
{
    private readonly ILogService _log;

    public StorageScanService(ILogService log) => _log = log;

    public IReadOnlyList<StorageVolumeInfo> GetVolumes()
    {
        var volumes = new List<StorageVolumeInfo>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (!drive.IsReady) continue;
                volumes.Add(new StorageVolumeInfo(drive.Name, drive.VolumeLabel, drive.DriveFormat, drive.TotalSize, drive.AvailableFreeSpace));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException)
            {
                _log.Warn($"No se pudo consultar la unidad {drive.Name}: {ex.Message}");
            }
        }

        return volumes;
    }

    public Task<StorageScanResult> ScanAsync(string rootPath, IProgress<StorageScanProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("La ruta de análisis es obligatoria.", nameof(rootPath));
        return Task.Run(() => Scan(rootPath, progress, cancellationToken), cancellationToken);
    }

    private StorageScanResult Scan(string rootPath, IProgress<StorageScanProgress>? progress, CancellationToken cancellationToken)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        if (!Directory.Exists(fullRoot)) throw new DirectoryNotFoundException($"No existe la carpeta: {fullRoot}");

        var items = new List<StorageScanItem>();
        var errors = new List<StorageScanError>();
        var pending = new Stack<string>();
        pending.Push(fullRoot);
        long files = 0, directories = 0, totalBytes = 0, nextProgressAt = 1;
        var rootAttributes = File.GetAttributes(fullRoot);
        var rootIsLink = (rootAttributes & FileAttributes.ReparsePoint) != 0;
        if (rootIsLink) errors.Add(Report(fullRoot, "La ruta raíz es un punto de reanálisis y no se seguirá."));
        else directories++;

        while (pending.Count > 0 && !rootIsLink)
        {
            if (cancellationToken.IsCancellationRequested) break;
            var current = pending.Pop();
            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(current).ToArray();
            }
            catch (Exception ex) when (IsExpectedFileSystemError(ex))
            {
                errors.Add(Report(current, ex.Message));
                continue;
            }

            foreach (var entry in entries)
            {
                if (cancellationToken.IsCancellationRequested) break;
                try
                {
                    var attributes = File.GetAttributes(entry);
                    var isDirectory = (attributes & FileAttributes.Directory) != 0;
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        errors.Add(Report(entry, "Punto de reanálisis omitido para evitar ciclos o doble contabilización."));
                        continue;
                    }

                    if (isDirectory)
                    {
                        directories++;
                        items.Add(new StorageScanItem(entry, true, 0, Directory.GetLastWriteTimeUtc(entry)));
                        pending.Push(entry);
                    }
                    else
                    {
                        var info = new FileInfo(entry);
                        var size = info.Length;
                        files++;
                        totalBytes = checked(totalBytes + size);
                        items.Add(new StorageScanItem(entry, false, size, info.LastWriteTimeUtc));
                    }
                }
                catch (Exception ex) when (IsExpectedFileSystemError(ex) || ex is ArgumentException or NotSupportedException or OverflowException)
                {
                    errors.Add(Report(entry, ex.Message));
                }

                if (files + directories >= nextProgressAt)
                {
                    progress?.Report(new StorageScanProgress(files, directories, current));
                    nextProgressAt = files + directories + 128;
                }
            }
        }

        progress?.Report(new StorageScanProgress(files, directories, pending.Count > 0 ? pending.Peek() : fullRoot));
        return new StorageScanResult(fullRoot, items, errors, files, directories, totalBytes, cancellationToken.IsCancellationRequested);
    }

    private StorageScanError Report(string path, string message)
    {
        _log.Warn($"Escaneo parcial en {path}: {message}");
        return new StorageScanError(path, message);
    }

    private static bool IsExpectedFileSystemError(Exception ex) =>
        ex is UnauthorizedAccessException or IOException or System.Security.SecurityException or System.IO.PathTooLongException;
}
