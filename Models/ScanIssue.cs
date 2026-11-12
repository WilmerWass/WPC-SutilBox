namespace Wpc_SutilBox.Models
{
    public class ScanIssue
    {
        public ScanCategory Category { get; set; }
        public IssueSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public bool IsSolvable { get; set; }
        public bool IsResolved { get; set; }
    }
}
