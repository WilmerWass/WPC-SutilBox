namespace Wpc_SutilBox.Models
{
    public class DashboardAlert
    {
        public IssueSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActionLabel { get; set; }
        public string? NavigateTo { get; set; }
    }
}
