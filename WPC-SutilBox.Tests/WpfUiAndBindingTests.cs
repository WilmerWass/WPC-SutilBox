using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;
using Wpc_SutilBox.ViewModels;
using Xunit;

namespace Wpc_SutilBox.Tests;

public class WpfUiAndBindingTests
{
    private sealed class BindingErrorListener : TraceListener
    {
        public List<string> Errors { get; } = new();

        public override void Write(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message) && !IsIgnored(message))
                Errors.Add(message);
        }

        public override void WriteLine(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message) && !IsIgnored(message))
                Errors.Add(message);
        }

        private static bool IsIgnored(string msg)
        {
            // Filter out benign trace messages that are not binding failures
            return msg.Contains("BindingExpression (hash=") && msg.Contains("AttachToContext");
        }
    }

    [Fact]
    public void MainWindow_AllSections_RenderWithoutBindingErrors()
    {
        Exception? threadException = null;
        List<string> bindingErrors = new();
        List<string> renderedSections = new();

        var thread = new Thread(() =>
        {
            var listener = new BindingErrorListener();
            try
            {
                PresentationTraceSources.Refresh();
                PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
                PresentationTraceSources.DataBindingSource.Listeners.Add(listener);

                App app;
                if (System.Windows.Application.Current is App existingApp)
                {
                    app = existingApp;
                }
                else
                {
                    app = new App();
                    app.InitializeComponent();
                }
                app.ChangeTheme(true);
                app.ChangeAccentColor("#4F46E5");

                var services = new ServiceCollection();
                app.ConfigureServices(services);
                using var provider = services.BuildServiceProvider();

                var mainWindow = provider.GetRequiredService<MainWindow>();
                var mainVm = provider.GetRequiredService<MainViewModel>();
                mainVm.GamesViewModel = provider.GetRequiredService<GamesViewModel>();
                mainWindow.DataContext = mainVm;

                mainWindow.Width = 1180;
                mainWindow.Height = 700;
                var content = (FrameworkElement)mainWindow.Content;
                content.Measure(new System.Windows.Size(1180, 700));
                content.Arrange(new Rect(0, 0, 1180, 700));
                content.UpdateLayout();
                PumpDispatcher();

                var screenshotDir = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..", "..", "test_screenshots", "ui_render"));
                Directory.CreateDirectory(screenshotDir);

                var sectionsToTest = new[]
                {
                    AppSection.Dashboard,
                    AppSection.RevisarPC,
                    AppSection.Limpieza,
                    AppSection.Desinstalador,
                    AppSection.Rendimiento,
                    AppSection.Privacidad,
                    AppSection.ZonaGamer,
                    AppSection.Herramientas,
                    AppSection.CentralDeshacer,
                    AppSection.Configuracion,
                    AppSection.Escaneo,
                    AppSection.TestRed,
                    AppSection.Almacenamiento,
                    AppSection.MapaEspacio,
                    AppSection.ArchivosDuplicados,
                    AppSection.ProgramasActualizar,
                    AppSection.InicioWindows,
                    AppSection.Optimizaciones,
                    AppSection.PuntosRestauracion,
                    AppSection.BackupFormatear,
                    AppSection.PostFormateo,
                    AppSection.Historial,
                    AppSection.Drivers,
                    AppSection.Batería,
                    AppSection.Benchmark,
                    AppSection.SaludDiscos,
                    AppSection.VerificarUSB,
                    AppSection.Periféricos,
                    AppSection.RepararWindows,
                    AppSection.CompatibilidadWin11,
                    AppSection.EditorPerfiles,
                    AppSection.AcercaDe
                };

                foreach (var section in sectionsToTest)
                {
                    Trace.WriteLine($"[TEST-RENDER] Testing section: {section}");
                    mainVm.CurrentSection = section;
                    content.UpdateLayout();
                    PumpDispatcher(30);

                    content.Measure(new System.Windows.Size(1180, 700));
                    content.Arrange(new Rect(0, 0, 1180, 700));
                    content.UpdateLayout();
                    PumpDispatcher(20);

                    Assert.Equal(section, mainVm.CurrentSection);

                    var rtb = new RenderTargetBitmap(1180, 700, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(content);

                    var pngPath = Path.Combine(screenshotDir, $"ui_{section}.png");
                    using (var fs = File.Create(pngPath))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        encoder.Save(fs);
                    }
                    renderedSections.Add(section.ToString());
                }

                // Minimum window size test (900x560)
                mainVm.CurrentSection = AppSection.Dashboard;
                content.UpdateLayout();
                PumpDispatcher(30);
                content.Measure(new System.Windows.Size(900, 560));
                content.Arrange(new Rect(0, 0, 900, 560));
                content.UpdateLayout();
                var rtbMin = new RenderTargetBitmap(900, 560, 96, 96, PixelFormats.Pbgra32);
                rtbMin.Render(content);
                using (var fs = File.Create(Path.Combine(screenshotDir, "ui_Dashboard_900x560.png")))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtbMin));
                    encoder.Save(fs);
                }

                mainWindow.AllowRealClose();
                mainWindow.Close();
                PumpDispatcher();
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
                bindingErrors = listener.Errors.ToList();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException);
        Assert.True(renderedSections.Count >= 32, $"Expected >= 32 rendered sections, got {renderedSections.Count}");
        Assert.True(bindingErrors.Count == 0,
            "WPF DataBinding errors detected:\n" + string.Join("\n", bindingErrors));
    }

    private static void PumpDispatcher(int waitMs = 30)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(waitMs)
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
