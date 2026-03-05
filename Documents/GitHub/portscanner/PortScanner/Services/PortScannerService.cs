using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using PortScanner.Models;
using PortScanner.Helpers;

namespace PortScanner.Services;

public class PortScannerService
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentBag<ScanResult> _results = new();
    private readonly Action<ScanResult>? _onResult;
    private readonly Stopwatch _stopwatch = new();

    public PortScannerService(int maxThreads, Action<ScanResult>? onResult = null)
    {
        _semaphore = new SemaphoreSlim(maxThreads, maxThreads);
        _onResult = onResult;
    }

    public async Task<IReadOnlyList<ScanResult>> ScanAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        _results.Clear();
        _stopwatch.Restart();

        IEnumerable<int> ports = options.UseWellKnownPortsOnly
            ? PortServiceMapping.WellKnownPorts
            : Enumerable.Range(options.StartPort, options.EndPort - options.StartPort + 1);

        var tasks = ports.Select(port => ScanPortAsync(options.Host, port, options.TimeoutMs, cancellationToken));
        await Task.WhenAll(tasks);

        _stopwatch.Stop();
        return _results.OrderBy(r => r.Port).ToList();
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    private async Task ScanPortAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var result = await TryConnectAsync(host, port, timeoutMs, cancellationToken);
            _results.Add(result);
            _onResult?.Invoke(result);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static async Task<ScanResult> TryConnectAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken)
    {
        var serviceName = PortServiceMapping.GetServiceName(port);
        var sw = Stopwatch.StartNew();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cts.Token);
            sw.Stop();

            return new ScanResult
            {
                Host = host,
                Port = port,
                IsOpen = true,
                ServiceName = serviceName,
                ResponseTimeMs = (int)sw.ElapsedMilliseconds,
                IsFiltered = false
            };
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            return new ScanResult
            {
                Host = host,
                Port = port,
                IsOpen = false,
                ServiceName = serviceName,
                ResponseTimeMs = null,
                IsFiltered = true
            };
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            sw.Stop();
            return new ScanResult
            {
                Host = host,
                Port = port,
                IsOpen = false,
                ServiceName = serviceName,
                ResponseTimeMs = (int)sw.ElapsedMilliseconds,
                IsFiltered = false
            };
        }
        catch (SocketException)
        {
            sw.Stop();
            return new ScanResult
            {
                Host = host,
                Port = port,
                IsOpen = false,
                ServiceName = serviceName,
                ResponseTimeMs = null,
                IsFiltered = true
            };
        }
        catch (Exception)
        {
            sw.Stop();
            return new ScanResult
            {
                Host = host,
                Port = port,
                IsOpen = false,
                ServiceName = serviceName,
                ResponseTimeMs = null,
                IsFiltered = true
            };
        }
    }

    public static async Task<string?> ResolveHostAsync(string hostOrIp, CancellationToken cancellationToken = default)
    {
        if (IPAddress.TryParse(hostOrIp, out _))
            return hostOrIp;
        try
        {
            var entry = await Dns.GetHostEntryAsync(hostOrIp, cancellationToken);
            return entry?.AddressList.FirstOrDefault()?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
