namespace Wpc_SutilBox.Models
{
    public sealed class CleanupCandidate
    {
        public CleanupCandidate(string path, long sizeBytes)
        {
            Path = path;
            SizeBytes = sizeBytes;
        }

        public string Path { get; }
        public long SizeBytes { get; }
    }
}
