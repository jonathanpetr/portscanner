using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PortScanner.Helpers;

public static class OsDetectionHelper
{
    /// <summary>
    /// Runs the system ping command and parses TTL from output (Windows/Linux).
    /// Uses TTL heuristics: ~64 = Linux, ~128 = Windows, ~255 = network device.
    /// </summary>
    public static async Task<(int? Ttl, string? OsGuess)> GetTtlFromPingCommandAsync(string host, CancellationToken cancellationToken = default)
    {
        var isWindows = OperatingSystem.IsWindows();
        var fileName = isWindows ? "ping" : "ping";
        var args = isWindows ? $"-n 1 -w 3000 {host}" : $"-c 1 -W 3 {host}";

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var ttl = ParseTtlFromPingOutput(output, isWindows);
            var guess = ttl.HasValue ? GuessOsFromTtl(ttl.Value) : null;
            return (ttl, guess);
        }
        catch
        {
            return (null, null);
        }
    }

    private static int? ParseTtlFromPingOutput(string output, bool isWindows)
    {
        if (isWindows)
        {
            var m = Regex.Match(output, @"TTL[=:](\d+)", RegexOptions.IgnoreCase);
            return m.Success && int.TryParse(m.Groups[1].Value, out var ttl) ? ttl : null;
        }
        var m2 = Regex.Match(output, @"ttl[=:](\d+)", RegexOptions.IgnoreCase);
        return m2.Success && int.TryParse(m2.Groups[1].Value, out var ttl2) ? ttl2 : null;
    }

    public static string GuessOsFromTtl(int ttl)
    {
        if (ttl <= 0) return "Unknown";
        if (ttl <= 64) return "Linux/Unix (or similar)";
        if (ttl <= 128) return "Windows";
        if (ttl <= 255) return "Network device / Router";
        return "Unknown";
    }
}
