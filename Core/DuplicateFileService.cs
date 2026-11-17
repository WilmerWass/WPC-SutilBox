using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public sealed class DuplicateFileService : IDuplicateFileService
    {
        private readonly ILogService _logService;

        public DuplicateFileService(ILogService logService) => _logService = logService;

        public async Task<DuplicateScanResult> FindDuplicatesAsync(
            string rootPath,
            IProgress<string>? progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
                throw new DirectoryNotFoundException("Selecciona una carpeta existente para analizar.");

            var filesByLength = new Dictionary<long, List<string>>();
            var pendingDirectories = new Stack<string>();
            pendingDirectories.Push(Path.GetFullPath(rootPath));
            int scannedFiles = 0;
            int skippedItems = 0;

            while (pendingDirectories.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string directory = pendingDirectories.Pop();

                try
                {
                    foreach (string filePath in Directory.EnumerateFiles(directory))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            var file = new FileInfo(filePath);
                            if (!filesByLength.TryGetValue(file.Length, out var sameLengthFiles))
                            {
                                sameLengthFiles = new List<string>();
                                filesByLength.Add(file.Length, sameLengthFiles);
                            }
                            sameLengthFiles.Add(file.FullName);
                            scannedFiles++;
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            skippedItems++;
                            _logService.Warn($"No se pudo inspeccionar el archivo '{filePath}': {ex.Message}");
                        }
                    }

                    foreach (string childDirectory in Directory.EnumerateDirectories(directory))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            if ((File.GetAttributes(childDirectory) & FileAttributes.ReparsePoint) == 0)
                                pendingDirectories.Push(childDirectory);
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            skippedItems++;
                            _logService.Warn($"No se pudo inspeccionar la carpeta '{childDirectory}': {ex.Message}");
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    skippedItems++;
                    _logService.Warn($"Se omitió la carpeta '{directory}': {ex.Message}");
                }

                if (scannedFiles % 100 == 0)
                    progress?.Report($"Carpetas pendientes: {pendingDirectories.Count:N0} · archivos revisados: {scannedFiles:N0}");
            }

            var duplicateFiles = new List<DuplicateFileRecord>();
            var hashes = new Dictionary<string, List<(string Path, long Size)>>(StringComparer.OrdinalIgnoreCase);
            int hashedFiles = 0;
            var hashCandidates = filesByLength.Values.Where(group => group.Count > 1).SelectMany(group => group);

            foreach (string filePath in hashCandidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    DateTime lastWrite = fileInfo.LastWriteTimeUtc;
                    long expectedLength = fileInfo.Length;
                    await using var stream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 1024 * 128,
                        useAsync: true);

                    byte[] digest = await SHA256.HashDataAsync(stream, cancellationToken);
                    fileInfo.Refresh();
                    if (stream.Length != expectedLength || fileInfo.LastWriteTimeUtc != lastWrite)
                    {
                        skippedItems++;
                        _logService.Warn($"Se omitió el archivo modificado durante el análisis: '{filePath}'.");
                        continue;
                    }

                    string hash = Convert.ToHexString(digest);
                    if (!hashes.TryGetValue(hash, out var matchingFiles))
                    {
                        matchingFiles = new List<(string Path, long Size)>();
                        hashes.Add(hash, matchingFiles);
                    }
                    matchingFiles.Add((filePath, expectedLength));
                    hashedFiles++;
                    if (hashedFiles % 25 == 0)
                        progress?.Report($"Comparando contenido: {hashedFiles:N0} archivos de tamaño coincidente...");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
                {
                    skippedItems++;
                    _logService.Warn($"No se pudo calcular el hash de '{filePath}': {ex.Message}");
                }
            }

            int groupNumber = 0;
            foreach (var (hash, group) in hashes.Where(pair => pair.Value.Count > 1))
            {
                groupNumber++;
                foreach (var file in group)
                {
                    duplicateFiles.Add(new DuplicateFileRecord
                    {
                        GroupNumber = groupNumber,
                        Hash = hash,
                        Path = file.Path,
                        SizeBytes = file.Size
                    });
                }
            }

            return new DuplicateScanResult
            {
                Files = duplicateFiles.OrderBy(file => file.GroupNumber).ThenBy(file => file.Path, StringComparer.OrdinalIgnoreCase).ToArray(),
                ScannedFiles = scannedFiles,
                SkippedItems = skippedItems,
                DuplicateGroups = groupNumber
            };
        }
    }
}
