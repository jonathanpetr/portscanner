namespace PortScanner.Models;

/// <summary>
/// Configuration options for a port scan.
/// </summary>
public class ScanOptions
{
    public required string Host { get; init; }
    public int StartPort { get; init; }
    public int EndPort { get; init; }
    public int TimeoutMs { get; init; } = 300;
    public int MaxThreads { get; init; } = 50;
    
    /// <summary>
    /// If true, only well-known ports are scanned (HTTP, HTTPS, SSH, etc.).
    /// </summary>
    public bool UseWellKnownPortsOnly { get; init; }
}
