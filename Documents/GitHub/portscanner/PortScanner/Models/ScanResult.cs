namespace PortScanner.Models;

/// <summary>
/// Represents the result of a single port scan attempt.
/// </summary>
public class ScanResult
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required bool IsOpen { get; init; }
    public required string ServiceName { get; init; }
    public int? ResponseTimeMs { get; init; }
    
    /// <summary>
    /// Indicates the port was filtered (timeout/unreachable) rather than explicitly closed.
    /// </summary>
    public bool IsFiltered { get; init; }
}
