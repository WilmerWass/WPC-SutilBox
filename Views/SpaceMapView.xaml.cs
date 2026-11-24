using System.Windows.Controls;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.ViewModels;

namespace Wpc_SutilBox.Views;

public partial class SpaceMapView : UserControl
{
    public SpaceMapView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SpaceMapViewModel)
            return;

        if (App.Current is App app
            && app.Services?.GetService(typeof(ISpaceMapService)) is ISpaceMapService service
            && app.Services?.GetService(typeof(IDialogService)) is IDialogService dialogs)
        {
            var viewModel = new SpaceMapViewModel(service, dialogs);
            viewModel.BackCommand = new RelayCommand(GoBack);
            RootLayout.DataContext = viewModel;
        }
    }

    private void GoBack()
    {
        if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel main)
            main.CurrentSection = Models.AppSection.Almacenamiento;
    }
}
