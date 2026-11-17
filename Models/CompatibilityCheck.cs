namespace Wpc_SutilBox.Models
{
    public sealed class CompatibilityCheck
    {
        public string Requirement { get; init; } = string.Empty;
        public string Result { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
    }
}
