using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wpc_SutilBox.Core;
using Wpc_SutilBox.Models;
using Wpc_SutilBox.ViewModels;
using Xunit;

namespace Wpc_SutilBox.Tests;

public class NavigationAndSecurityTests
{
    [Theory]
    [InlineData(AppSection.ProgramasInstalar, AppSection.ProgramasActualizar)]
    [InlineData(AppSection.Bloatware, AppSection.Desinstalador)]
    [InlineData(AppSection.Red, AppSection.TestRed)]
    [InlineData(AppSection.Hardware, AppSection.RevisarPC)]
    [InlineData(AppSection.Escala, AppSection.Configuracion)]
    [InlineData(AppSection.Dashboard, AppSection.Dashboard)]
    [InlineData(AppSection.ZonaGamer, AppSection.ZonaGamer)]
    [InlineData(AppSection.Limpieza, AppSection.Limpieza)]
    public void AppSectionRoutes_Canonicalize_ResolvesCorrectly(AppSection input, AppSection expected)
    {
        var result = AppSectionRoutes.Canonicalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task ProcessManagerService_ProtectsCriticalPids(int pid)
    {
        var log = new TestLogService();
        var mgr = new ProcessManagerService(log);
        bool result = await mgr.KillProcessAsync(pid);
        Assert.False(result);
        Assert.Contains(log.Messages, m => m.Contains($"Blocked attempt to terminate protected PID {pid}"));
    }

    [Fact]
    public async Task ProcessManagerService_ProtectsCriticalProcessesByName()
    {
        var log = new TestLogService();
        var mgr = new ProcessManagerService(log);

        var explorer = System.Diagnostics.Process.GetProcessesByName("explorer");
        if (explorer.Length > 0)
        {
            int explorerPid = explorer[0].Id;
            bool killed = await mgr.KillProcessAsync(explorerPid);
            Assert.False(killed);
            Assert.Contains(log.Messages, m => m.Contains("Blocked attempt to terminate protected process explorer"));
        }
    }

    [Fact]
    public void DashboardViewModel_HealthScore_ReturnsSinDatos_WhenNoData()
    {
        var monitoring = new TestMonitoringService();
        var temp = new TestTempService();
        var battery = new TestBatteryService();
        var disk = new TestDiskHealthService();
        var sec = new TestSecurityService();
        var sys = new TestSystemInfoService();
        var startup = new TestStartupService();
        var storage = new TestStorageScanService();
        var scan = new TestSystemScanService();

        var vm = new DashboardViewModel(monitoring, temp, battery, disk, sec, sys, startup, storage, scan);

        Assert.Null(vm.HealthScore);
        Assert.Equal("Sin datos", vm.HealthScoreDisplay);
    }

    [Fact]
    public async Task GameLibraryService_DeduplicatesAndVerifiesManifests()
    {
        var service = new GameLibraryService();
        var games = await service.DetectInstalledGamesAsync(CancellationToken.None);

        Assert.NotNull(games);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in games)
        {
            Assert.True(paths.Add(g.InstallPath), $"Duplicate game install path: {g.InstallPath}");
            Assert.False(string.IsNullOrWhiteSpace(g.Name));
            Assert.False(string.IsNullOrWhiteSpace(g.Platform));
            Assert.True(Directory.Exists(g.InstallPath), $"Game path must exist: {g.InstallPath}");
        }
    }

    [Fact]
    public async Task PcReviewViewModel_LeavesCpuTemperatureUnavailableWhenSensorHasNoReading()
    {
        var vm = new PcReviewViewModel(
            new TestSystemInfoService(),
            new TestDiskHealthService(),
            new TestDriverService(),
            new TestMonitoringService(),
            new TestTempService());

        await vm.LoadDataAsync();

        Assert.Null(vm.CpuTempC);
    }

    #region Mock Services for Testing
    private class TestLogService : ILogService
    {
        public List<string> Messages { get; } = new();
        public void Info(string message) => Messages.Add($"INFO: {message}");
        public void Warn(string message) => Messages.Add($"WARN: {message}");
        public void Error(string message, Exception? ex = null) => Messages.Add($"ERROR: {message} | {ex?.Message}");
    }

    private class TestMonitoringService : IMonitoringService
    {
        public Task StartAsync() => Task.CompletedTask;
        public Task StopAsync() => Task.CompletedTask;
        public Task<SystemUsage> GetSystemUsageAsync() => Task.FromResult(new SystemUsage { CpuUsage = 15, RamUsage = 40, DiskUsage = 50 });
        public Task<GlobalUsageSnapshot> GetGlobalUsageAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new GlobalUsageSnapshot { CpuUsage = 15.0, RamUsage = 40.0 });
        public TimeSpan GetIdleTime() => TimeSpan.Zero;
        public void Dispose() { }
    }

    private class TestTempService : ITemperatureMonitorService
    {
        public Task<double?> GetCpuTemperatureCAsync() => Task.FromResult<double?>(null);
    }

    private class TestBatteryService : IBatteryService
    {
        public Task<BatteryInfo> GetBatteryStatusAsync() => Task.FromResult(new BatteryInfo());
    }

    private class TestDiskHealthService : IDiskHealthService
    {
        public Task<IEnumerable<DiskHealthInfo>> GetDiskHealthAsync() =>
            Task.FromResult<IEnumerable<DiskHealthInfo>>(Array.Empty<DiskHealthInfo>());
    }

    private class TestSecurityService : ISecurityService
    {
        public Task<SecurityStatus> GetSecurityStatusAsync() => Task.FromResult(new SecurityStatus());
        public Task<bool> IsRealTimeProtectionEnabledAsync() => Task.FromResult(true);
        public Task<bool> IsFirewallEnabledAsync() => Task.FromResult(true);
    }

    private class TestSystemInfoService : ISystemInfoService
    {
        public Task<SystemInfo> GetSystemInfoAsync() => Task.FromResult(new SystemInfo());
    }

    private class TestStartupService : IStartupService
    {
        public Task<IEnumerable<StartupItem>> GetStartupItemsAsync() =>
            Task.FromResult<IEnumerable<StartupItem>>(Array.Empty<StartupItem>());
        public Task<bool> EnableStartupItemAsync(StartupItem item) => Task.FromResult(true);
        public Task<bool> DisableStartupItemAsync(StartupItem item) => Task.FromResult(true);
    }

    private class TestStorageScanService : IStorageScanService
    {
        public IReadOnlyList<StorageVolumeInfo> GetVolumes() => Array.Empty<StorageVolumeInfo>();
        public Task<StorageScanResult> ScanAsync(string rootPath, IProgress<StorageScanProgress>? progress = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new StorageScanResult("C:\\", Array.Empty<StorageScanItem>(), Array.Empty<StorageScanError>(), 0, 0, 0, false));
    }

    private class TestSystemScanService : ISystemScanService
    {
        public Task<ScanResult> RunFullScanAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new ScanResult());
    }

    private class TestDriverService : IDriverService
    {
        public Task<(bool Success, string Message)> ExportDriversAsync(
            string destinationPath,
            IProgress<(int, string)> progress,
            CancellationToken cancellationToken) =>
            Task.FromResult((true, string.Empty));

        public Task<List<DriverInfo>> GetDriversWithProblemsAsync() =>
            Task.FromResult(new List<DriverInfo>());
    }
    #endregion
}
