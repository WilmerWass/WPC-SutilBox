using System.Collections.Generic;

namespace SutilBox.CLI.Models;

public class ScanResult
{
    public DateTime Timestamp { get; set; }
    public List<ScanIssue> Issues { get; set; } = new();
    public int HealthScore { get; set; }
    public Dictionary<ScanCategory, int> CategorySummary { get; set; } = new();
}
