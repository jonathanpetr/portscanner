using PortScanner.Models;

namespace PortScanner.Helpers;

public static class ConsoleHelper
{
    public static void WriteResult(ScanResult result, bool showClosedAndFiltered = true)
    {
        var (label, color) = result.IsOpen
            ? ("OPEN", ConsoleColor.Green)
            : result.IsFiltered
                ? ("FILTERED", ConsoleColor.Yellow)
                : ("CLOSED", ConsoleColor.Red);

        if (!result.IsOpen && !showClosedAndFiltered)
            return;

        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write($"[{label,-8}] ");
        Console.ForegroundColor = prev;
        Console.Write($"{result.Port,5}   → {result.ServiceName}");
        if (result.IsOpen && result.ResponseTimeMs.HasValue)
            Console.Write($" ({result.ResponseTimeMs}ms)");
        Console.WriteLine();
    }

    public static void WriteHeader(string host, int startPort, int endPort, bool wellKnownOnly)
    {
        var range = wellKnownOnly ? "well-known ports" : $"ports {startPort}-{endPort}";
        Console.WriteLine();
        Console.WriteLine($"Scanning {host} ({range})...");
        Console.WriteLine();
    }

    public static void WriteSummary(int openCount, int totalScanned, TimeSpan duration, string host, string? osGuess = null)
    {
        Console.WriteLine("---------------------------");
        Console.WriteLine($"Scan complete: {openCount} open port(s) found (of {totalScanned} scanned) in {duration.TotalSeconds:F1}s");
        Console.WriteLine($"Host: {host}");
        if (!string.IsNullOrEmpty(osGuess))
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"OS guess (TTL): {osGuess}");
            Console.ForegroundColor = prev;
        }
        Console.WriteLine();
    }

    public static void WriteError(string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Fehler: {message}");
        Console.ForegroundColor = prev;
    }

    public static void WriteInfo(string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ForegroundColor = prev;
    }
}
