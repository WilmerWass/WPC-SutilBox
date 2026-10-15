using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices; // Interopservices
using System.IO.Enumeration;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core
{
    public class SystemMaintenanceService : ISystemMaintenanceService
    {
        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        [DllImport("shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, RecycleFlags dwFlags);

        [Flags]
        enum RecycleFlags : int
        {
            SHERB_NOCONFIRMATION = 0x00000001,  // No pedir confirmación
            SHERB_NOPROGRESSUI = 0x00000002,    // No mostrar cuadro de progreso
            SHERB_NOSOUND = 0x00000004          // No reproducir sonido de papelera de reciclaje
        }

        private static readonly string[] TempDirectories = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), // caché de IE obsoleto
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
            // Cachés de navegador
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\Default\\Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla\\Firefox\\Profiles"), // Necesita una búsqueda más profunda
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data\\Default\\Cache"),
            // Archivos temporales de Windows Update
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution\\Download"),
        };

        private static readonly string[] TempFilePatterns = new[] { "*.tmp", "*.temp", "*.log", "*.bak", "*.old", "*.gid", "*.chk" };

        public async Task<CleanResult> CleanTemporaryFilesAsync(CleaningOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            return await Task.Run(() => CleanTemporaryFiles(options));
        }

        public CleanResult CleanTemporaryFiles(CleaningOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            var candidates = PreviewTemporaryFiles(options);
            return CleanTemporaryFiles(candidates);
        }

        public async Task<IReadOnlyList<CleanupCandidate>> PreviewTemporaryFilesAsync(
            CleaningOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);
            return await Task.Run(() => PreviewTemporaryFiles(options, cancellationToken), cancellationToken);
        }

        public IReadOnlyList<CleanupCandidate> PreviewTemporaryFiles(
            CleaningOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);
            var candidates = new List<CleanupCandidate>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var directory in GetCandidateDirectories(options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsSafeCandidateDirectory(directory) || !Directory.Exists(directory) || !visited.Add(directory))
                    continue;

                foreach (var file in SafeEnumerateTemporaryFiles(directory))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var info = new FileInfo(file);
                        if (!TempFilePatterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, info.Name, ignoreCase: true)))
                            continue;
                        if (info.Exists && !IsReparsePoint(info))
                            candidates.Add(new CleanupCandidate(info.FullName, info.Length));
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }

            return candidates
                .GroupBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
        }

        public async Task<CleanResult> CleanTemporaryFilesAsync(
            IReadOnlyCollection<CleanupCandidate> candidates,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(candidates);
            return await Task.Run(() => CleanTemporaryFiles(candidates, cancellationToken), cancellationToken);
        }

        public CleanResult CleanTemporaryFiles(
            IReadOnlyCollection<CleanupCandidate> candidates,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(candidates);
            var result = new CleanResult();

            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (candidate == null || !IsSafeCandidateFile(candidate.Path))
                {
                    result.FilesFailed++;
                    continue;
                }

                try
                {
                    var info = new FileInfo(candidate.Path);
                    if (!info.Exists || IsReparsePoint(info))
                        continue;

                    long size = info.Length;
                    File.Delete(info.FullName);
                    result.FilesDeleted++;
                    result.BytesFreed += size;
                }
                catch (IOException) { result.FilesFailed++; }
                catch (UnauthorizedAccessException) { result.FilesFailed++; }
            }

            return result;
        }

        private static IEnumerable<string> GetCandidateDirectories(CleaningOptions options)
        {
            if (!options.CleanSystemTemp)
                yield break;

            foreach (var directory in TempDirectories.Take(4))
                yield return directory;

            var usersDir = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "Users");
            if (!Directory.Exists(usersDir))
                yield break;

            foreach (var user in Directory.GetDirectories(usersDir))
            {
                yield return Path.Combine(user, "AppData\\Local\\Temp");
                yield return Path.Combine(user, "AppData\\Local\\Microsoft\\Windows\\INetCache");
            }
        }

        private static IEnumerable<string> SafeEnumerateTemporaryFiles(string root)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                IEnumerable<string> files;
                try { files = Directory.EnumerateFiles(current); }
                catch { continue; }

                foreach (var file in files)
                {
                    if (TempFilePatterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, Path.GetFileName(file), ignoreCase: true)))
                        yield return file;
                }

                IEnumerable<string> directories;
                try { directories = Directory.EnumerateDirectories(current); }
                catch { continue; }

                foreach (var directory in directories)
                {
                    try
                    {
                        var info = new DirectoryInfo(directory);
                        if (!IsReparsePoint(info)) pending.Push(directory);
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }

        private static bool IsSafeCandidateDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return false;
            string fullPath;
            try { fullPath = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; }
            catch { return false; }

            var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (string.IsNullOrWhiteSpace(windowsDirectory)) return false;
            string windows = Path.GetFullPath(windowsDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string users = Path.GetFullPath(Path.Combine(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "Users")).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string currentTemp = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var allowedRoots = new List<string> { currentTemp, Path.Combine(windows, "Temp") + Path.DirectorySeparatorChar };
            if (Directory.Exists(users.TrimEnd(Path.DirectorySeparatorChar)))
            {
                foreach (var profile in Directory.GetDirectories(users.TrimEnd(Path.DirectorySeparatorChar)))
                {
                    allowedRoots.Add(Path.Combine(profile, "AppData", "Local", "Temp") + Path.DirectorySeparatorChar);
                    allowedRoots.Add(Path.Combine(profile, "AppData", "Local", "Microsoft", "Windows", "INetCache") + Path.DirectorySeparatorChar);
                }
            }
            return allowedRoots.Any(root => fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsSafeCandidateFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            try
            {
                var fullPath = Path.GetFullPath(filePath);
                var directory = Path.GetDirectoryName(fullPath);
                return directory != null && IsSafeCandidateDirectory(directory) &&
                       TempFilePatterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, Path.GetFileName(fullPath), ignoreCase: true));
            }
            catch { return false; }
        }

        private static bool IsReparsePoint(FileSystemInfo info) =>
            (info.Attributes & FileAttributes.ReparsePoint) != 0;

        private CleanResult CleanRecycleBin()
        {
            var result = new CleanResult();
            try
            {
                SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
                result.Notes += "Papelera de reciclaje vaciada.";
                // No podemos obtener fácilmente los bytes liberados para la papelera de reciclaje, así que solo informaremos del éxito.
            }
            catch (Exception ex)
            {
                // Registrar y continuar, no fallar toda la limpieza
                // _log?.Error($"Error limpiando Papelera de Reciclaje: {ex.Message}");
                result.FilesFailed++;// Indicar un problema
                result.Notes += $"Error al vaciar papelera: {ex.Message}";
            }
            return result;
        }

        public async Task<CleanResult> OptimizeMemoryAsync()
        {
             return await Task.Run(() => OptimizeMemory());
        }

        public CleanResult OptimizeMemory()
        {
            try 
            {
                // Simplemente ejecutaremos la optimización.
                
                // Simplemente ejecutaremos la optimización.
                var processes = Process.GetProcesses();
                foreach (var p in processes)
                {
                    try 
                    {
                        if (!p.HasExited)
                        {
                            EmptyWorkingSet(p.Handle);
                        }
                    } 
                    catch { } // Ignorar acceso denegado, etc.
                }

                // Como marcador de posición para los bytes liberados, no podemos calcularlos fácilmente sin verificar la memoria global antes y después.
                // Devolveremos 0 bytes pero estableceremos un mensaje de éxito.
                return new CleanResult { BytesFreed = 0, Notes = "Procesos optimizados." };
            }
            catch (Exception ex)
            {
                 return new CleanResult { FilesFailed = 1, Notes = ex.Message };
            }
        }

        public async Task OptimizeSelfAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // 1. Forzar recolección de basura
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // 2. Liberar Working Set (Memoria en Task Manager)
                    EmptyWorkingSet(Process.GetCurrentProcess().Handle);
                }
                catch { }
            });
        }

        private CleanResult Aggregate(CleanResult r1, CleanResult r2)
        {
            return new CleanResult
            {
                FilesDeleted = r1.FilesDeleted + r2.FilesDeleted,
                FoldersDeleted = r1.FoldersDeleted + r2.FoldersDeleted,
                BytesFreed = r1.BytesFreed + r2.BytesFreed,
                FilesFailed = r1.FilesFailed + r2.FilesFailed,
                FoldersFailed = r1.FoldersFailed + r2.FoldersFailed
            };
        }

        private CleanResult CleanDirectory(string dir)
        {
            var result = new CleanResult();

            try
            {
                foreach (var pattern in TempFilePatterns)
                {
                    foreach (var file in SafeEnumerateFiles(dir, pattern, SearchOption.AllDirectories))
                    {
                        try
                        {
                            var fi = new FileInfo(file);
                            long size = fi.Exists ? fi.Length : 0;
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                            result.FilesDeleted++;
                            result.BytesFreed += size;
                        }
                        catch
                        {
                            result.FilesFailed++;
                        }
                    }
                }

                // Eliminar directorios vacíos
                foreach (var sub in SafeEnumerateDirectories(dir, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
                {
                    try
                    {
                        if (Directory.Exists(sub) && !Directory.EnumerateFileSystemEntries(sub).Any())
                        {
                            Directory.Delete(sub, false);
                            result.FoldersDeleted++;
                        }
                    }
                    catch
                    {
                        result.FoldersFailed++;
                    }
                }
            }
            catch
            {
                // no fatal
            }

            return result;
        }

        private static IEnumerable<string> SafeEnumerateFiles(string path, string searchPattern, SearchOption option)
        {
            var pending = new Stack<string>();
            pending.Push(path);
            while (pending.Count > 0)
            {
                string current = pending.Pop();
                IEnumerable<string> files = Enumerable.Empty<string>();
                try { files = Directory.EnumerateFiles(current, searchPattern); } catch { }
                foreach (var f in files) yield return f;
                if (option == SearchOption.AllDirectories)
                {
                    IEnumerable<string> dirs = Enumerable.Empty<string>();
                    try { dirs = Directory.EnumerateDirectories(current); } catch { }
                    foreach (var d in dirs) pending.Push(d);
                }
            }
        }

        private static IEnumerable<string> SafeEnumerateDirectories(string path, string searchPattern, SearchOption option)
        {
            var pending = new Stack<string>();
            pending.Push(path);
            while (pending.Count > 0)
            {
                string current = pending.Pop();
                IEnumerable<string> dirs = Enumerable.Empty<string>();
                try { dirs = Directory.EnumerateDirectories(current, searchPattern); } catch { }
                foreach (var d in dirs)
                {
                    yield return d;
                    if (option == SearchOption.AllDirectories)
                    {
                        pending.Push(d);
                    }
                }
            }
        }

        public async Task<ProcessLaunchResult> FlushDnsAsync()
        {
            return await Task.Run(() => LaunchElevated("cmd.exe", "/c ipconfig /flushdns"));
        }

        public async Task<ProcessLaunchResult> AnalyzeDiskAsync()
        {
            return await Task.Run(() =>
            {
                string? sysDrive = Path.GetPathRoot(Environment.SystemDirectory);
                if (string.IsNullOrEmpty(sysDrive))
                {
                    return new ProcessLaunchResult { Started = false, Message = "No se pudo determinar la unidad del sistema." };
                }
                // /A = Analizar, /V = Verbose
                return LaunchElevated("cmd.exe", $"/c defrag {sysDrive.TrimEnd('\\')} /A /V");
            });
        }

        public async Task<ProcessLaunchResult> CleanPrefetchAsync()
        {
            // Requiere Admin. Intentamos borrar archivos en C:\Windows\Prefetch
            return await Task.Run(() =>
            {
                var result = new ProcessLaunchResult { Started = true };
            try
            {
                string prefetchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                if (Directory.Exists(prefetchPath))
                {
                    int deleted = 0;
                    int failed = 0;
                    foreach (var file in Directory.EnumerateFiles(prefetchPath))
                    {
                        try
                        {
                            File.Delete(file);
                            deleted++;
                        }
                        catch
                        {
                            failed++;
                        }
                    }
                    result.Message = $"Limpieza de Prefetch completada. {deleted} archivos borrados, {failed} omitidos (en uso o sin permisos).";
                    result.ExitCode = 0;
                }
                else
                {
                    result.Message = "No se encontró el directorio Prefetch.";
                    result.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                result.Started = false;
                result.Message = $"Error al limpiar Prefetch: {ex.Message}";
                result.StandardError = ex.ToString();
            }
            return result;
            });
        }

        public async Task<ProcessLaunchResult> RebuildSearchIndexAsync()
        {
            // La forma segura es abrir el panel de control
            return await Task.Run(() => LaunchElevated("control.exe", "srchadmin.dll"));
        }

        public async Task<ProcessLaunchResult> ResetNetworkAsync()
        {
            // Ejecutar una secuencia de comandos de red
            return await Task.Run(() => LaunchElevated("cmd.exe", "/c ipconfig /release && ipconfig /renew && ipconfig /flushdns && netsh int ip reset && netsh winsock reset"));
        }

        public bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public async Task<ProcessLaunchResult> LaunchSystemFileCheckerAsync()
        {
            // sfc /scannow debe ejecutarse con privilegios elevados; abrimos una consola elevada si no somos administradores
            return await Task.Run(() => LaunchElevated("cmd.exe", "/c sfc /scannow"));
        }

        public async Task<ProcessLaunchResult> LaunchDISMHealthRestoreAsync()
        {
            // Restauración de salud de DISM
            return await Task.Run(() => LaunchElevated("cmd.exe", "/c DISM /Online /Cleanup-Image /RestoreHealth"));
        }

        public async Task<ProcessLaunchResult> LaunchCHKDSKAsync()
        {
            // Programar chkdsk en el próximo arranque para la unidad del sistema
            return await Task.Run(() =>
            {
                string? sysDrive = Path.GetPathRoot(Environment.SystemDirectory);
                if (string.IsNullOrEmpty(sysDrive))
                {
                    return new ProcessLaunchResult { Started = false, Message = "No se pudo determinar la unidad del sistema." };
                }
                // Usamos echo Y | para confirmar automáticamente la programación del análisis en el próximo reinicio
                return LaunchElevated("cmd.exe", $"/c echo Y | chkdsk {sysDrive.TrimEnd('\\')} /F /R");
            });
        }

        private ProcessLaunchResult LaunchElevated(string fileName, string arguments)
        {
            Process? p = null;
            string standardOutput = string.Empty;
            string standardError = string.Empty;
            int? exitCode = null;
            try
            {
                if (IsAdministrator())
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    p = Process.Start(psi);
                    if (p == null) return new ProcessLaunchResult { Started = false, Message = "No se pudo iniciar el proceso." };
                    standardOutput = p.StandardOutput.ReadToEnd();
                    standardError = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                    return new ProcessLaunchResult
                    {
                        Started = true,
                        ExitCode = exitCode,
                        Message = $"Proceso completado. Código de salida: {exitCode}.",
                        StandardOutput = standardOutput,
                        StandardError = standardError
                    };
                }
                else
                {
                    var psCommand = $"Start-Process -FilePath \\\"{fileName}\\\" -ArgumentList \\\"{arguments}\\\" -Verb RunAs";
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    p = Process.Start(psi);
                    if (p == null) return new ProcessLaunchResult { Started = false, Message = "No se pudo solicitar elevación." };
                    standardOutput = p.StandardOutput.ReadToEnd();
                    standardError = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                    return new ProcessLaunchResult
                    {
                        Started = exitCode == 0,
                        ExitCode = exitCode,
                        Message = "Se solicitó elevación. Acepte el cuadro de UAC para continuar.",
                        StandardOutput = standardOutput,
                        StandardError = standardError
                    };
                }
            }
            catch (Exception ex)
            {
                return new ProcessLaunchResult { Started = false, Message = $"Error al iniciar o ejecutar el proceso: {ex.Message}", StandardError = ex.ToString() };
            }
            finally
            {
                p?.Dispose();
            }
        }
    }
}

