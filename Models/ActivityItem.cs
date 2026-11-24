namespace Wpc_SutilBox.Models
{
    public class ActivityItem
    {
        public DateTime Timestamp { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = "ℹ️";
    }
}
