using System.Windows.Controls;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.ViewModels;

namespace Wpc_SutilBox.Views;

public partial class SpaceMapView : UserControl
{
    public SpaceMapView()
    {
        InitializeComponent();
        if (App.Current is App app && app.Services.GetService(typeof(ISpaceMapService)) is ISpaceMapService service && app.Services.GetService(typeof(IDialogService)) is IDialogService dialogs)
        {
            var viewModel = new SpaceMapViewModel(service, dialogs);
            viewModel.BackCommand = new RelayCommand(GoBack);
            DataContext = viewModel;
        }
    }

    private void GoBack()
    {
        if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel main)
            main.CurrentSection = "Almacenamiento";
    }
}
