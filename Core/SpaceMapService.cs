using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core;

public sealed class SpaceMapService : ISpaceMapService
{
    private const int MaxEntries = 100_000;
    private readonly ILogService? _log;

    public SpaceMapService(ILogService? log = null)
    {
        _log = log;
    }

    public Task<IReadOnlyList<SpaceMapEntry>> AnalyzeAsync(IEnumerable<string> roots, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roots);
        var rootSnapshot = roots.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        _log?.Info($"[SpaceMap] Iniciando análisis para {rootSnapshot.Length} raíz/raíces.");
        return Task.Run<IReadOnlyList<SpaceMapEntry>>(() => rootSnapshot.Select(root => AnalyzeRoot(root, cancellationToken)).Where(root => root != null).Cast<SpaceMapEntry>().ToArray(), cancellationToken);
    }

    public Task<SpaceMapDeleteResult> DeleteSelectedAsync(IReadOnlyCollection<SpaceMapEntry> entries, IEnumerable<string> authorizedRoots, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(authorizedRoots);
        var snapshot = entries.ToArray();
        var requestedRoots = authorizedRoots.Where(path => !string.IsNullOrWhiteSpace(path)).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var tempRoots = GetTemporaryRoots();
        var roots = requestedRoots.Where(requested => tempRoots.Any(temp => IsSameOrDescendant(requested, temp))).ToArray();
        return Task.Run(() => Delete(snapshot, roots, cancellationToken), cancellationToken);
    }

    private SpaceMapEntry? AnalyzeRoot(string root, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string fullRoot;
        try { fullRoot = Path.GetFullPath(root); }
        catch (Exception ex) { _log?.Warn($"[SpaceMap] Ruta no válida '{root}': {ex.Message}"); return null; }
        var rootInfo = new DirectoryInfo(fullRoot);
        try { if (!rootInfo.Exists || IsReparse(rootInfo)) return null; }
        catch (Exception ex) { _log?.Warn($"[SpaceMap] Error comprobando raíz '{fullRoot}': {ex.Message}"); return null; }

        var rootEntry = new SpaceMapEntry { Name = rootInfo.Name.Length == 0 ? fullRoot : rootInfo.Name, Path = fullRoot, Category = "Carpeta analizada", IsDirectory = true };
        var pending = new Stack<(DirectoryInfo Directory, SpaceMapEntry Node)>();
        pending.Push((rootInfo, rootEntry));
        var entries = 0;
        while (pending.Count > 0 && entries < MaxEntries)
        {
            token.ThrowIfCancellationRequested();
            var (directory, parent) = pending.Pop();
            IEnumerable<FileSystemInfo> children;
            try { children = directory.EnumerateFileSystemInfos().ToArray(); }
            catch (IOException ex) { _log?.Warn($"[SpaceMap] Bloqueo de E/S en carpeta '{directory.FullName}': {ex.Message}"); continue; }
            catch (UnauthorizedAccessException ex) { _log?.Warn($"[SpaceMap] Acceso denegado en carpeta '{directory.FullName}': {ex.Message}"); continue; }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
                if (++entries >= MaxEntries) break;
                try
                {
                    if (IsReparse(child)) continue;
                    if (child is DirectoryInfo subdirectory)
                    {
                        var node = new SpaceMapEntry { Name = subdirectory.Name, Path = subdirectory.FullName, Category = "Carpeta", IsDirectory = true, LastModified = subdirectory.LastWriteTimeUtc };
                        parent.Children.Add(node);
                        pending.Push((subdirectory, node));
                    }
                    else if (child is FileInfo file)
                    {
                        long size;
                        DateTime modified;
                        try { size = file.Length; modified = file.LastWriteTimeUtc; }
                        catch (IOException) { continue; }
                        var node = new SpaceMapEntry { Name = file.Name, Path = file.FullName, Category = Categorize(file.Extension), IsDirectory = false, SizeBytes = size, LastModified = modified };
                        parent.Children.Add(node);
                    }
                }
                catch (IOException ex) { _log?.Warn($"[SpaceMap] Bloqueo de E/S procesando elemento: {ex.Message}"); }
                catch (UnauthorizedAccessException ex) { _log?.Warn($"[SpaceMap] Acceso no autorizado procesando elemento: {ex.Message}"); }
            }
        }
        CalculateDirectorySizes(rootEntry);
        return rootEntry;
    }

    private static long CalculateDirectorySizes(SpaceMapEntry entry)
    {
        if (!entry.IsDirectory) return entry.SizeBytes;
        long total = 0;
        foreach (var child in entry.Children) total = SaturatingAdd(total, CalculateDirectorySizes(child));
        SetComputedSize(entry, total);
        return total;
    }

    private static void SetComputedSize(SpaceMapEntry entry, long size)
    {
        entry.SizeBytes = size;
    }

    private static long SaturatingAdd(long left, long right) => left > long.MaxValue - right ? long.MaxValue : left + right;

    private static SpaceMapDeleteResult Delete(IReadOnlyCollection<SpaceMapEntry> entries, IReadOnlyCollection<string> authorizedRoots, CancellationToken token)
    {
        int deleted = 0, skipped = 0, failed = 0;
        long bytes = 0;
        foreach (var entry in entries)
        {
            token.ThrowIfCancellationRequested();
            if (entry.IsDirectory || !IsAllowedFile(entry.Path, authorizedRoots)) { failed++; continue; }
            try
            {
                var info = new FileInfo(Path.GetFullPath(entry.Path));
                if (!info.Exists || IsReparse(info)) { skipped++; continue; }
                var parent = info.Directory;
                if (parent == null || IsReparse(parent) || !IsWithinAuthorizedRoot(parent.FullName, authorizedRoots) || IsProtectedPath(info.FullName)) { failed++; continue; }
                var actualSize = info.Length;
                File.Delete(info.FullName);
                deleted++;
                bytes = SaturatingAdd(bytes, actualSize);
            }
            catch (FileNotFoundException) { skipped++; }
            catch (DirectoryNotFoundException) { skipped++; }
            catch (IOException) { failed++; }
            catch (UnauthorizedAccessException) { failed++; }
            catch (ArgumentException) { failed++; }
            catch (NotSupportedException) { failed++; }
        }
        return new SpaceMapDeleteResult(deleted, skipped, failed, bytes);
    }

    private static bool IsAllowedFile(string path, IReadOnlyCollection<string> authorizedRoots)
    {
        try
        {
            var full = Path.GetFullPath(path);
            var parent = Path.GetDirectoryName(full);
            return parent != null && IsWithinAuthorizedRoot(parent, authorizedRoots) && !IsProtectedPath(full);
        }
        catch { return false; }
    }

    private static bool IsWithinAuthorizedRoot(string directory, IReadOnlyCollection<string> authorizedRoots)
    {
        string full = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return authorizedRoots.Any(root =>
        {
            var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return full.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static bool IsSameOrDescendant(string path, string root)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProtectedPath(string path)
    {
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (!string.IsNullOrWhiteSpace(windows))
        {
            var protectedRoot = Path.GetFullPath(windows).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalized = Path.GetFullPath(path);
            if (normalized.StartsWith(protectedRoot, StringComparison.OrdinalIgnoreCase) &&
                !normalized.StartsWith(Path.Combine(protectedRoot, "Temp") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static string[] GetTemporaryRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Add(string path)
        {
            try
            {
                var full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var info = new DirectoryInfo(full);
                if (info.Exists && !IsReparse(info)) roots.Add(full);
            }
            catch { }
        }
        Add(Path.GetTempPath());
        Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));
        var users = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "Users");
        try
        {
            foreach (var profile in Directory.EnumerateDirectories(users))
            {
                Add(Path.Combine(profile, "AppData", "Local", "Temp"));
                Add(Path.Combine(profile, "AppData", "Local", "Microsoft", "Windows", "INetCache"));
            }
        }
        catch { }
        return roots.ToArray();
    }

    private static bool IsReparse(FileSystemInfo info)
    {
        try { return (info.Attributes & FileAttributes.ReparsePoint) != 0; }
        catch { return true; }
    }

    private static string Categorize(string extension) => extension.ToLowerInvariant() switch
    {
        ".mp4" or ".mkv" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => "Video",
        ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".m4a" => "Audio",
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" or ".svg" or ".jfif" or ".ico" => "Imagen",
        ".exe" or ".msi" or ".msp" or ".bat" or ".cmd" or ".ps1" => "Ejecutable",
        ".zip" or ".7z" or ".rar" or ".tar" or ".gz" or ".bz2" => "Comprimido",
        ".iso" or ".img" or ".vmdk" or ".vhd" or ".vhdx" => "Imagen de disco",
        ".pdf" or ".docx" or ".xlsx" or ".pptx" or ".txt" or ".csv" or ".doc" or ".xls" => "Documento",
        ".tmp" or ".temp" or ".cache" or ".bak" or ".old" or ".dmp" => "Temporal",
        ".cs" or ".js" or ".ts" or ".html" or ".css" or ".json" or ".xml" or ".py" or ".cpp" => "Código",
        ".log" => "Registro",
        _ => "Archivo"
    };
}
