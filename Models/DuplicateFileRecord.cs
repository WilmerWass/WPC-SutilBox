namespace Wpc_SutilBox.Models
{
    public sealed class DuplicateFileRecord
    {
        public int GroupNumber { get; init; }
        public string Hash { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
    }

    public sealed class DuplicateScanResult
    {
        public IReadOnlyList<DuplicateFileRecord> Files { get; init; } = Array.Empty<DuplicateFileRecord>();
        public int ScannedFiles { get; init; }
        public int SkippedItems { get; init; }
        public int DuplicateGroups { get; init; }
    }
}
