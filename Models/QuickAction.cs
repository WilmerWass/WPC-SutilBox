using System.Windows.Input;

namespace Wpc_SutilBox.Models
{
    public class QuickAction
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string NavigateTo { get; set; } = string.Empty;
        public ICommand? DirectCommand { get; set; }
    }
}
