using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public enum HistoryCategory
    {
        Cleanup,
        Uninstall,
        Optimization,
        Driver,
        Registry,
        Service,
        Startup,
        Privacy,
        RestorePoint,
        Other
    }

    public class HistoryEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public HistoryCategory Category { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ReversibleState { get; set; }
        public bool IsReversible { get; set; }
        public bool WasReverted { get; set; }
    }

    public interface IHistoryService
    {
        Task AddEntryAsync(HistoryEntry entry);
        Task<List<HistoryEntry>> GetEntriesAsync();
        Task<List<HistoryEntry>> GetEntriesByCategoryAsync(HistoryCategory category);
        Task<bool> RevertEntryAsync(Guid id);
        Task ClearAsync();
        Task ClearRevertedAsync();
    }
}
