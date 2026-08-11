using System;
using System.ComponentModel;
using System.Windows;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.ViewModels;

namespace Wpc_SutilBox;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogService _logService;

    private bool _sidebarExpanded = true;
    private bool _allowClose;

    public MainWindow(ILogService logService)
    {
        _logService = logService;

        InitializeComponent();

        _logService.Info("MainWindow inicializado - WPC-SutilBox Beta.1");

        WindowState = WindowState.Normal;

        IsVisibleChanged += MainWindow_IsVisibleChanged;
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateViewModelWindowState();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        UpdateViewModelWindowState();
    }

    private void UpdateViewModelWindowState()
    {
        if (DataContext is MainViewModel vm)
        {
            vm.IsWindowVisible =
                IsVisible &&
                WindowState != WindowState.Minimized;
        }

        _logService.Info(
            $"Window State Changed - Visible: {IsVisible}, State: {WindowState}");
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _sidebarExpanded = !_sidebarExpanded;

        SidebarColumn.Width = _sidebarExpanded
            ? new GridLength(235)
            : new GridLength(72);

        _logService.Info(
            $"Sidebar {( _sidebarExpanded ? "expandida" : "contraída" )}");
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_allowClose ||
            Application.Current is App app && app.IsShuttingDown)
        {
            base.OnClosing(e);
            return;
        }

        if (DataContext is MainViewModel vm)
        {
            vm.MinimizeToTray();

            e.Cancel = true;
            Hide();

            return;
        }

        base.OnClosing(e);
    }

    public void AllowRealClose()
    {
        _allowClose = true;
    }
}