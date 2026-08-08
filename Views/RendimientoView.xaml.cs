using System.Windows.Controls;
using Wpc_SutilBox.ViewModels;

namespace Wpc_SutilBox.Views
{
    public partial class RendimientoView : UserControl
    {
        public RendimientoView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    await vm.UpdateSystemUsageAsync();
                }
            };
        }
    }
}
