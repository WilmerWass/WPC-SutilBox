using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Wpc_SutilBox.Core
{
    public class NetworkTestResult
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? Target { get; set; }
        public double? PingMs { get; set; }
        public bool PacketLoss { get; set; }
        public int PacketsSent { get; set; }
        public int PacketsReceived { get; set; }
        public string? DnsServer { get; set; }
        public double? DnsResolutionMs { get; set; }
        public double? DownloadSpeedMbps { get; set; }
        public double? UploadSpeedMbps { get; set; }
        public string? Error { get; set; }
    }

    public interface INetworkTestService
    {
        Task<NetworkTestResult> RunNetworkTestAsync(string target = "8.8.8.8", CancellationToken cancellationToken = default);
        Task<double?> PingAsync(string host, int timeoutMs = 1000, CancellationToken cancellationToken = default);
        Task<(string Server, double Ms)> ResolveDnsAsync(string domain = "www.google.com", CancellationToken cancellationToken = default);
        Task<double?> MeasureDownloadSpeedAsync(CancellationToken cancellationToken = default);
        Task<double?> MeasureUploadSpeedAsync(CancellationToken cancellationToken = default);
    }

    public class NetworkTestService : INetworkTestService
    {
        private readonly ILogService _logService;

        public NetworkTestService(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<NetworkTestResult> RunNetworkTestAsync(string target = "8.8.8.8", CancellationToken cancellationToken = default)
        {
            var result = new NetworkTestResult { Target = target };

            try
            {
                result.PingMs = await PingAsync(target, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Error = "Cancelado.";
                    return result;
                }

                var dnsResult = await ResolveDnsAsync(cancellationToken: cancellationToken);
                result.DnsServer = dnsResult.Server;
                result.DnsResolutionMs = dnsResult.Ms;
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Error = "Cancelado.";
                    return result;
                }

                result.DownloadSpeedMbps = await MeasureDownloadSpeedAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Error = "Cancelado.";
                    return result;
                }

                result.UploadSpeedMbps = await MeasureUploadSpeedAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                result.Error = "Prueba cancelada.";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logService.Error("Error en prueba de red", ex);
            }

            return result;
        }

        public async Task<double?> PingAsync(string host, int timeoutMs = 1000, CancellationToken cancellationToken = default)
        {
            try
            {
                using var ping = new Ping();
                var options = new PingOptions { DontFragment = true };
                var buffer = new byte[32];

                var reply = await Task.Run(() => ping.Send(host, timeoutMs, buffer, options), cancellationToken);
                if (reply.Status == IPStatus.Success)
                {
                    return reply.RoundtripTime;
                }
            }
            catch (Exception ex)
            {
                _logService.Warn($"Ping fallido a {host}: {ex.Message}");
            }
            return null;
        }

        public async Task<(string Server, double Ms)> ResolveDnsAsync(string domain = "www.google.com", CancellationToken cancellationToken = default)
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                string dnsServer = "No disponible";
                var adapters = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var adapter in adapters)
                {
                    var ipProps = adapter.GetIPProperties();
                    var dnsServers = ipProps.DnsAddresses;
                    if (dnsServers.Count > 0)
                    {
                        dnsServer = dnsServers[0].ToString();
                        break;
                    }
                }

                await Task.Run(() => Dns.GetHostEntry(domain), cancellationToken);
                sw.Stop();

                return (dnsServer, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logService.Warn($"DNS resolution failed for {domain}: {ex.Message}");
                return ("Error", 0);
            }
        }

        public async Task<double?> MeasureDownloadSpeedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var handler = new HttpClientHandler();
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var url = "https://speed.cloudflare.com/__down?bytes=5000000";

                var data = await client.GetByteArrayAsync(url, cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > 0)
                {
                    double bytesPerMs = data.Length / (double)sw.ElapsedMilliseconds;
                    double mbps = (bytesPerMs * 8) / 1024.0 / 1024.0 * 1000.0;
                    return Math.Round(mbps, 2);
                }
            }
            catch (Exception ex)
            {
                _logService.Warn($"Download speed test failed: {ex.Message}");
            }
            return null;
        }

        public async Task<double?> MeasureUploadSpeedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var handler = new HttpClientHandler();
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var url = "https://speed.cloudflare.com/__up";
                var data = new byte[5000000];

                await client.PostAsync(url, new ByteArrayContent(data), cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > 0)
                {
                    double bytesPerMs = data.Length / (double)sw.ElapsedMilliseconds;
                    double mbps = (bytesPerMs * 8) / 1024.0 / 1024.0 * 1000.0;
                    return Math.Round(mbps, 2);
                }
            }
            catch (Exception ex)
            {
                _logService.Warn($"Upload speed test failed: {ex.Message}");
            }
            return null;
        }
    }
}
