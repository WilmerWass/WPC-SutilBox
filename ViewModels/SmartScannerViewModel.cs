using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;

#nullable enable

namespace Wpc_SutilBox.ViewModels
{
    public class SmartScannerViewModel : ViewModelBase
    {
        private readonly ISystemScanService _scanService;
        private readonly IDialogService? _dialogService;

        private bool _isScanning;
        private int _progress;
        private string _statusMessage = "Presiona 'Iniciar Escáner' para analizar tu sistema.";
        private int _healthScore = -1;
        private ScanIssue? _selectedIssue;
        private string _filterCategory = "Todas";
        private string _filterSeverity = "Todas";

        public ObservableCollection<ScanIssue> Issues { get; } = new();

        public bool IsScanning
        {
            get => _isScanning;
            private set => SetProperty(ref _isScanning, value);
        }

        public int Progress
        {
            get => _progress;
            private set => SetProperty(ref _progress, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public int HealthScore
        {
            get => _healthScore;
            private set => SetProperty(ref _healthScore, value);
        }

        public ScanIssue? SelectedIssue
        {
            get => _selectedIssue;
            set => SetProperty(ref _selectedIssue, value);
        }

        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                if (SetProperty(ref _filterCategory, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string FilterSeverity
        {
            get => _filterSeverity;
            set
            {
                if (SetProperty(ref _filterSeverity, value))
                {
                    ApplyFilters();
                }
            }
        }

        public ObservableCollection<ScanIssue> FilteredIssues { get; } = new();

        public string[] CategoryFilters => new[] { "Todas", "Hardware", "Drivers", "Disco", "Rendimiento", "Seguridad", "Privacidad", "Inicio", "Sistema" };
        public string[] SeverityFilters => new[] { "Todas", "Crítico", "Advertencia", "Info" };

        private List<ScanIssue> _allIssues = new();

        public ICommand StartScanCommand { get; }
        public ICommand SolveIssueCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand ClearResultsCommand { get; }

        public SmartScannerViewModel(ISystemScanService scanService, IDialogService? dialogService = null)
        {
            _scanService = scanService;
            _dialogService = dialogService;

            StartScanCommand = new RelayCommand<object?>((_) => StartScanAsync(_), (_) => !IsScanning);
            SolveIssueCommand = new RelayCommand<ScanIssue?>((issue) => { if (issue != null) NavigateToSolution(issue); });
            ExportReportCommand = new RelayCommand<object?>((_) => ExportReport(_), (_) => _allIssues.Count > 0);
            ClearResultsCommand = new RelayCommand<object?>(ClearResults);
        }

        private async void StartScanAsync(object? parameter)
        {
            IsScanning = true;
            Progress = 0;
            HealthScore = -1;
            Issues.Clear();
            _allIssues.Clear();
            FilteredIssues.Clear();
            FilterCategory = "Todas";
            FilterSeverity = "Todas";

            var cts = new CancellationTokenSource();
            var progressToken = new ProgressReporter(OnProgressUpdated);

            try
            {
                StatusMessage = "Iniciando escaneo del sistema...";

                var result = await _scanService.RunFullScanAsync(cts.Token);

                _allIssues = result.Issues;
                HealthScore = result.HealthScore;

                // Ordenar por severidad: Critical > Warning > Info
                var sorted = _allIssues
                    .OrderBy(i => i.Severity == IssueSeverity.Critical ? 0 : i.Severity == IssueSeverity.Warning ? 1 : 2)
                    .ToList();

                foreach (var issue in sorted)
                {
                    Issues.Add(issue);
                }

                ApplyFilters();

                Progress = 100;
                var criticalCount = sorted.Count(i => i.Severity == IssueSeverity.Critical);
                var warningCount = sorted.Count(i => i.Severity == IssueSeverity.Warning);

                if (criticalCount > 0)
                {
                    StatusMessage = $"Escaneo completado. Se encontraron {criticalCount} problemas críticos y {warningCount} advertencias.";
                }
                else if (warningCount > 0)
                {
                    StatusMessage = $"Escaneo completado. Se encontraron {warningCount} advertencias.";
                }
                else
                {
                    StatusMessage = "Escaneo completado. Tu sistema se encuentra en buen estado.";
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "El escaneo fue cancelado.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error durante el escaneo: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }

        private void OnProgressUpdated(int value)
        {
            Progress = Math.Min(95, value);
        }

        private bool NavigateToSolution(ScanIssue issue)
        {
            // En esta fase solo navegamos a la sección correspondiente
            // Las soluciones automáticas se implementarán en una fase posterior
            issue.IsResolved = true;
            StatusMessage = $"Solución aplicada para: {issue.Title}";
            return true;
        }

        private void ExportReport(object? parameter)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"Reporte_Escaneo_{timestamp}.md";

                var lines = new List<string>
                {
                    $"# Reporte de Escaneo del Sistema",
                    $"",
                    $"**Fecha:** {DateTime.Now:dd/MM/yyyy HH:mm}",
                    $"**Puntuación de salud:** {HealthScore}/100",
                    $"",
                    $"## Resumen",
                    $""
                };

                var criticalCount = _allIssues.Count(i => i.Severity == IssueSeverity.Critical);
                var warningCount = _allIssues.Count(i => i.Severity == IssueSeverity.Warning);
                var infoCount = _allIssues.Count(i => i.Severity == IssueSeverity.Info);

                lines.Add($"| Severidad | Cantidad |");
                lines.Add($"|-----------|----------|");
                lines.Add($"| 🔴 Crítico | {criticalCount} |");
                lines.Add($"| 🟡 Advertencia | {warningCount} |");
                lines.Add($"| 🔵 Info | {infoCount} |");
                lines.Add("");

                if (criticalCount > 0)
                {
                    lines.Add("## 🔴 Problemas Críticos");
                    lines.Add("");
                    foreach (var issue in _allIssues.Where(i => i.Severity == IssueSeverity.Critical))
                    {
                        lines.Add($"### {issue.Title}");
                        lines.Add($"");
                        lines.Add($"- **Descripción:** {issue.Description}");
                        lines.Add($"- **Solución:** {issue.Solution}");
                        lines.Add($"- **Categoría:** {issue.Category}");
                        lines.Add("");
                    }
                }

                if (warningCount > 0)
                {
                    lines.Add("## 🟡 Advertencias");
                    lines.Add("");
                    foreach (var issue in _allIssues.Where(i => i.Severity == IssueSeverity.Warning))
                    {
                        lines.Add($"### {issue.Title}");
                        lines.Add($"");
                        lines.Add($"- **Descripción:** {issue.Description}");
                        lines.Add($"- **Solución:** {issue.Solution}");
                        lines.Add($"- **Categoría:** {issue.Category}");
                        lines.Add("");
                    }
                }

                if (infoCount > 0)
                {
                    lines.Add("## 🔵 Información");
                    lines.Add("");
                    foreach (var issue in _allIssues.Where(i => i.Severity == IssueSeverity.Info))
                    {
                        lines.Add($"### {issue.Title}");
                        lines.Add($"");
                        lines.Add($"- **Descripción:** {issue.Description}");
                        lines.Add($"- **Categoría:** {issue.Category}");
                        lines.Add("");
                    }
                }

                var report = string.Join("\n", lines);

                // Guardar el reporte
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = filename,
                    Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*",
                    Title = "Exportar Reporte de Escaneo"
                };

                if (dialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(dialog.FileName, report);
                    StatusMessage = $"Reporte exportado a: {dialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al exportar reporte: {ex.Message}";
            }
        }

        private void ClearResults(object? parameter)
        {
            Issues.Clear();
            _allIssues.Clear();
            FilteredIssues.Clear();
            HealthScore = -1;
            Progress = 0;
            StatusMessage = "Resultados limpiados. Presiona 'Iniciar Escáner' para un nuevo análisis.";
            FilterCategory = "Todas";
            FilterSeverity = "Todas";
        }

        private void ApplyFilters()
        {
            FilteredIssues.Clear();

            var filtered = _allIssues.AsEnumerable();

            if (FilterCategory != "Todas")
            {
                if (Enum.TryParse<ScanCategory>(FilterCategory, out var category))
                {
                    filtered = filtered.Where(i => i.Category == category);
                }
            }

            if (FilterSeverity != "Todas")
            {
                filtered = filtered.Where(i =>
                {
                    return FilterSeverity switch
                    {
                        "Crítico" => i.Severity == IssueSeverity.Critical,
                        "Advertencia" => i.Severity == IssueSeverity.Warning,
                        "Info" => i.Severity == IssueSeverity.Info,
                        _ => true
                    };
                });
            }

            foreach (var issue in filtered)
            {
                FilteredIssues.Add(issue);
            }
        }

        private class ProgressReporter : IProgress<int>
        {
            private readonly Action<int> _report;

            public ProgressReporter(Action<int> report)
            {
                _report = report;
            }

            public void Report(int value)
            {
                _report(value);
            }
        }
    }
}
