using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.ViewModels;
using System.Drawing;
using System.Windows.Forms;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Runtime.InteropServices;

namespace Wpc_SutilBox  
{
    public partial class App : Application
    {
        // P/Invoke para Win32 API
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private readonly ServiceProvider? _serviceProvider;
        private NotifyIcon? _notifyIcon;
        private System.Threading.Mutex? _instanceMutex;
        public bool IsShuttingDown { get; private set; }

        public App()
        {
            // Verificar instancia única
            bool createdNew;
            _instanceMutex = new System.Threading.Mutex(true, "Wpc_SutilBox_SingleInstance_Mutex", out createdNew);
            
            if (!createdNew)
            {
                ActivateExistingInstance();
                Current.Shutdown();
                return;
            }

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ActivateExistingInstance()
        {
            try
            {
                var current = System.Diagnostics.Process.GetCurrentProcess();
                var previous = System.Diagnostics.Process.GetProcessesByName(current.ProcessName)
                    .FirstOrDefault(p => p.Id != current.Id);

                if (previous != null)
                {
                    IntPtr handle = previous.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        ShowWindow(handle, SW_RESTORE);
                        SetForegroundWindow(handle);
                    }
                }
            }
            catch { }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMonitoringService, MonitoringService>();
            services.AddSingleton<IPerformanceProfileService, PerformanceProfileService>();
            services.AddSingleton<IProcessManagerService, ProcessManagerService>();
            services.AddSingleton<ITemperatureMonitorService, TemperatureMonitorService>();
            services.AddSingleton<IDiskHealthService, DiskHealthService>();
            services.AddSingleton<ISecurityService, SecurityService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ILogService, FileLogService>();
            services.AddSingleton<ISystemInfoService, SystemInfoService>();
            services.AddSingleton<ISystemMaintenanceService, SystemMaintenanceService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IServiceOptimizerService, ServiceOptimizerService>();
            services.AddSingleton<IBloatwareService, BloatwareService>();
            services.AddSingleton<IPrivacyService, PrivacyService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IRestorePointService, RestorePointService>();
            services.AddSingleton<IBatteryService, BatteryService>();
            services.AddSingleton<IWingetService, WingetService>();
            services.AddSingleton<IDriverService, DriverService>();
            services.AddSingleton<IDiskAnalyzerService, DiskAnalyzerService>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>(s => new MainWindow(s.GetRequiredService<ILogService>()));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Captura errores globales de UI vinculados a tu WriteLog
            DispatcherUnhandledException += (s, args) =>
            {
                MainViewModel.WriteLog("FALLO CRÍTICO EN UI", args.Exception);
                System.Windows.MessageBox.Show($"Ocurrió un error inesperado: {args.Exception.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    MainViewModel.WriteLog("FALLO CRÍTICO NO CONTROLADO (Background)", ex);
                }
            };

            var settings = _serviceProvider!.GetService<ISettingsService>();
            var loc = _serviceProvider!.GetService<ILocalizationService>();
            var log = _serviceProvider!.GetService<ILogService>();
            
            try
            {
                if (settings != null)
                {
                    var s = await settings.LoadAsync();
                    if (s != null && loc != null)
                    {
                        await loc.SetLanguageAsync(s.Language);
                        ChangeAccentColor(s.AccentColor);
                        ChangeTheme(s.IsDarkMode);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Error("Error durante la inicialización de ajustes", ex);
            }

            try
            {
                var mainWindow = _serviceProvider!.GetRequiredService<MainWindow>();
                mainWindow.DataContext = _serviceProvider!.GetRequiredService<MainViewModel>();
                this.MainWindow = mainWindow;

                SetupTrayIcon();
                mainWindow.Show();
                ShowMainWindow();
            }
            catch (Exception ex)
            {
                log?.Error("CRITICAL ERROR during MainWindow startup", ex);
                System.Windows.MessageBox.Show($"Error crítico al iniciar la ventana principal:\n{ex.Message}", "Error de Inicio", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void SetupTrayIcon()
        {
            try
            {
                _notifyIcon = new NotifyIcon();
                try
                {
                    string assemblyLocation = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                    if (string.IsNullOrEmpty(assemblyLocation) || !System.IO.File.Exists(assemblyLocation))
                    {
                        _notifyIcon.Icon = SystemIcons.Application;
                    }
                    else
                    {
                        _notifyIcon.Icon = Icon.ExtractAssociatedIcon(assemblyLocation);
                    }
                }
                catch
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }

                _notifyIcon.Text = "Wpc_SutilBox";
                _notifyIcon.Visible = true;
                _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("🚀 Optimizar PC", null, (s, e) => (MainWindow?.DataContext as MainViewModel)?.PcBoostCommand?.Execute(null));
                contextMenu.Items.Add("🧹 Limpiar RAM", null, (s, e) => (MainWindow?.DataContext as MainViewModel)?.OptimizeRamCommand?.Execute(null));
                contextMenu.Items.Add("-");
                
                var navMenu = new ToolStripMenuItem("Navegar a...");
                navMenu.DropDownItems.Add("Inicio", null, (s, e) => NavigateToSection("Dashboard"));
                navMenu.DropDownItems.Add("Protección", null, (s, e) => NavigateToSection("Proteccion"));
                navMenu.DropDownItems.Add("Rendimiento", null, (s, e) => NavigateToSection("Rendimiento"));
                navMenu.DropDownItems.Add("Hardware", null, (s, e) => NavigateToSection("Hardware"));
                contextMenu.Items.Add(navMenu);

                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Restaurar App", null, (s, e) => ShowMainWindow());
                contextMenu.Items.Add("Salir", null, (s, e) => ShutdownApp());
                
                _notifyIcon.ContextMenuStrip = contextMenu;
            }
            catch (Exception ex)
            {
                var log = _serviceProvider?.GetService<ILogService>();
                log?.Error("Error al configurar el icono de bandeja", ex);
            }
        }

        private void NavigateToSection(string section)
        {
            ShowMainWindow();
            (MainWindow?.DataContext as MainViewModel)?.NavigateCommand.Execute(section);
        }

        public void ShowMainWindow()
        {
            if (MainWindow != null)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    ShowWindow(new System.Windows.Interop.WindowInteropHelper(MainWindow).Handle, SW_RESTORE);
                }
                MainWindow.Show();
                SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(MainWindow).Handle);
            }
        }

        private async void ShutdownApp()
        {
            IsShuttingDown = true;
            if (MainWindow?.DataContext is MainViewModel vm)
            {
                await vm.PrepareForShutdownAsync();
            }
            _notifyIcon?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!IsShuttingDown && MainWindow?.DataContext is MainViewModel vm)
            {
                Task.Run(async () => await vm.PrepareForShutdownAsync()).Wait(3000);
            }

            _notifyIcon?.Dispose();
            _instanceMutex?.ReleaseMutex();
            _instanceMutex?.Dispose();
            base.OnExit(e);
        }

        public void ChangeAccentColor(string hexColor)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                var solidBrush = new SolidColorBrush(color);
                
                byte r = (byte)Math.Min(255, color.R + 30);
                byte g = (byte)Math.Min(255, color.G + 30);
                byte b = (byte)Math.Min(255, color.B + 30);
                var hoverColor = Color.FromRgb(r, g, b);
                var hoverBrush = new SolidColorBrush(hoverColor);

                Resources["PrimaryColor"] = color;
                Resources["PrimaryBrush"] = solidBrush;
                Resources["PrimaryHoverColor"] = hoverColor;
                Resources["PrimaryHoverBrush"] = hoverBrush;
                
                var selectionColor = Color.FromArgb(40, color.R, color.G, color.B);
                Resources["SelectionColor"] = selectionColor;
                Resources["SelectionBrush"] = new SolidColorBrush(selectionColor);
            }
            catch { }
        }

        public void ChangeTheme(bool isDark)
        {
            try
            {
                var nonThemeDictionaries = Resources.MergedDictionaries
                    .Where(d => d.Source == null || !d.Source.OriginalString.Contains("Theme."))
                    .ToList();

                Resources.MergedDictionaries.Clear();

                foreach (var dict in nonThemeDictionaries)
                {
                    Resources.MergedDictionaries.Add(dict);
                }

                var themeName = isDark ? "Theme.Dark.xaml" : "Theme.Light.xaml";
                var uri = new Uri($"Resources/{themeName}", UriKind.Relative);
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });

                if (isDark)
                {
                    Resources["SurfaceHoverColor"] = (Color)ColorConverter.ConvertFromString("#2A2A2A");
                }
                else
                {
                    Resources["SurfaceHoverColor"] = (Color)ColorConverter.ConvertFromString("#E5E7EB");
                }
                Resources["SurfaceHoverBrush"] = new SolidColorBrush((Color)Resources["SurfaceHoverColor"]);
            }
            catch { }
        }
    }
}
