using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public class HistoryService : IHistoryService
    {
        private readonly string _dir;
        private readonly string _file;
        private readonly ILogService _logService;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            AllowTrailingCommas = true
        };

        public HistoryService(ILogService logService, string? appFolderName = null)
        {
            _logService = logService;
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dir = Path.Combine(appData, appFolderName ?? "Wpc_SutilBox", "history");
            Directory.CreateDirectory(_dir);
            _file = Path.Combine(_dir, "history.json");
        }

        public async Task AddEntryAsync(HistoryEntry entry)
        {
            try
            {
                var entries = await LoadEntriesAsync();
                entries.Insert(0, entry);
                await SaveEntriesAsync(entries);
                _logService.Info($"Historial: {entry.Title} - {entry.Description}");
            }
            catch (Exception ex)
            {
                _logService.Error($"Error al añadir entrada al historial: {entry.Title}", ex);
            }
        }

        public async Task<List<HistoryEntry>> GetEntriesAsync()
        {
            return await LoadEntriesAsync();
        }

        public async Task<List<HistoryEntry>> GetEntriesByCategoryAsync(HistoryCategory category)
        {
            var entries = await LoadEntriesAsync();
            return entries.Where(e => e.Category == category).ToList();
        }

        public async Task<bool> RevertEntryAsync(Guid id)
        {
            try
            {
                var entries = await LoadEntriesAsync();
                var entry = entries.FirstOrDefault(e => e.Id == id);
                if (entry == null) return false;
                if (!entry.IsReversible) return false;

                // La reversión real depende de la categoría y el estado guardado
                // Cada servicio que registre una entrada reversible debe implementar
                // la lógica de reversión específica. Aquí marcamos como revertida.
                entry.WasReverted = true;
                await SaveEntriesAsync(entries);
                _logService.Info($"Historial: entrada {entry.Title} marcada como revertida");
                return true;
            }
            catch (Exception ex)
            {
                _logService.Error($"Error al revertir entrada del historial: {id}", ex);
                return false;
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                await File.WriteAllTextAsync(_file, "[]");
                _logService.Info("Historial: todas las entradas eliminadas");
            }
            catch (Exception ex)
            {
                _logService.Error("Error al limpiar el historial", ex);
            }
        }

        public async Task ClearRevertedAsync()
        {
            try
            {
                var entries = await LoadEntriesAsync();
                entries = entries.Where(e => !e.WasReverted).ToList();
                await SaveEntriesAsync(entries);
                _logService.Info("Historial: entradas revertidas eliminadas");
            }
            catch (Exception ex)
            {
                _logService.Error("Error al limpiar entradas revertidas", ex);
            }
        }

        private async Task<List<HistoryEntry>> LoadEntriesAsync()
        {
            try
            {
                if (!File.Exists(_file))
                {
                    return new List<HistoryEntry>();
                }
                string json = await File.ReadAllTextAsync(_file);
                var entries = JsonSerializer.Deserialize<List<HistoryEntry>>(json, _jsonOptions);
                return entries ?? new List<HistoryEntry>();
            }
            catch (Exception ex)
            {
                _logService.Error("Error al cargar el historial", ex);
                return new List<HistoryEntry>();
            }
        }

        private async Task SaveEntriesAsync(List<HistoryEntry> entries)
        {
            try
            {
                Directory.CreateDirectory(_dir);
                string json = JsonSerializer.Serialize(entries, _jsonOptions);
                await File.WriteAllTextAsync(_file, json);
            }
            catch (Exception ex)
            {
                _logService.Error("Error al guardar el historial", ex);
            }
        }
    }
}
