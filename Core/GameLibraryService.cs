using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core;

public sealed class GameLibraryService : IGameLibraryService
{
    public Task<IReadOnlyList<GameInstallation>> DetectInstalledGamesAsync(CancellationToken cancellationToken = default) =>
        Task.Run<IReadOnlyList<GameInstallation>>(() =>
        {
            var games = new Dictionary<string, GameInstallation>(StringComparer.OrdinalIgnoreCase);
            foreach (var library in GetSteamLibraries())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var manifests = Path.Combine(library, "steamapps");
                if (!Directory.Exists(manifests)) continue;
                IEnumerable<string> files;
                try { files = Directory.EnumerateFiles(manifests, "appmanifest_*.acf").ToArray(); }
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var text = File.ReadAllText(file);
                        var name = MatchValue(text, "name");
                        var installDir = MatchValue(text, "installdir");
                        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(installDir)) continue;
                        var path = Path.GetFullPath(Path.Combine(manifests, "common", installDir));
                        if (Directory.Exists(path))
                            games.TryAdd(path, new GameInstallation(name, "Steam", path, "Instalación verificada por manifiesto y carpeta"));
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                    catch (ArgumentException) { }
                }
            }

            var epicManifests = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Epic", "EpicGamesLauncher", "Data", "Manifests");
            try
            {
                if (Directory.Exists(epicManifests))
                    foreach (var file in Directory.EnumerateFiles(epicManifests, "*.item"))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            using var json = JsonDocument.Parse(File.ReadAllText(file));
                            var root = json.RootElement;
                            if (!root.TryGetProperty("DisplayName", out var display) ||
                                !root.TryGetProperty("InstallLocation", out var install)) continue;
                            var name = display.GetString();
                            var path = install.GetString();
                            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                            {
                                var full = Path.GetFullPath(path);
                                games.TryAdd(full, new GameInstallation(name, "Epic Games", full, "Instalación verificada por manifiesto y carpeta"));
                            }
                        }
                        catch (JsonException) { }
                        catch (IOException) { }
                        catch (UnauthorizedAccessException) { }
                        catch (ArgumentException) { }
                    }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return (IReadOnlyList<GameInstallation>)games.Values.OrderBy(game => game.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        }, cancellationToken);

    private static IEnumerable<string> GetSteamLibraries()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
        if (!string.IsNullOrWhiteSpace(steamPath)) roots.Add(steamPath.Replace('/', '\\'));
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFiles)) roots.Add(Path.Combine(programFiles, "Steam"));
        foreach (var root in roots.ToArray())
        {
            yield return root;
            var libraryFile = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile)) continue;
            string text;
            try { text = File.ReadAllText(libraryFile); }
            catch (IOException) { continue; }
            catch (UnauthorizedAccessException) { continue; }
            foreach (Match match in Regex.Matches(text, "\\\"path\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase))
                yield return match.Groups[1].Value.Replace("\\\\", "\\");
        }
    }

    private static string? MatchValue(string text, string key)
    {
        var match = Regex.Match(text, "\\\"" + Regex.Escape(key) + "\\\"\\s+\\\"([^\\\"]*)\\\"", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }
}
