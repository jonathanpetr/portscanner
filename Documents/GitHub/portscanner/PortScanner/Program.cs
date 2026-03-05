using PortScanner.Models;
using PortScanner.Services;
using PortScanner.Helpers;

var (host, startPort, endPort, timeoutMs, maxThreads, wellKnownOnly, showAll, outputPath) = ParseArguments(args);

if (host is null)
{
    await RunInteractiveModeAsync();
    return;
}

var resolved = await PortScannerService.ResolveHostAsync(host);
if (resolved is null)
{
    ConsoleHelper.WriteError($"Host '{host}' konnte nicht aufgelöst werden. Bitte prüfen Sie die Adresse.");
    return 1;
}

var options = new ScanOptions
{
    Host = resolved,
    StartPort = startPort!.Value,
    EndPort = endPort!.Value,
    TimeoutMs = timeoutMs,
    MaxThreads = maxThreads,
    UseWellKnownPortsOnly = wellKnownOnly
};

ConsoleHelper.WriteHeader(host, options.StartPort, options.EndPort, wellKnownOnly);

var lockObj = new object();
var scanner = new PortScannerService(maxThreads, result =>
{
    lock (lockObj)
        ConsoleHelper.WriteResult(result, showAll);
});

var results = await scanner.ScanAsync(options);
var openCount = results.Count(r => r.IsOpen);
var totalScanned = results.Count;

string? osGuess = null;
try
{
    var (_, ttl, guess) = await OsDetectionHelper.GetTtlFromPingCommandAsync(resolved);
    if (guess != null) osGuess = ttl.HasValue ? $"{guess} (TTL={ttl})" : guess;
}
catch { /* OS detection is best-effort */ }

ConsoleHelper.WriteSummary(openCount, totalScanned, scanner.Elapsed, host, osGuess);

if (!string.IsNullOrEmpty(outputPath))
{
    OutputExportHelper.Export(results, outputPath, host, scanner.Elapsed);
    ConsoleHelper.WriteInfo($"Ergebnisse gespeichert: {Path.GetFullPath(outputPath)}");
}

return 0;

// --- Argument parsing ---
static (string? host, int? startPort, int? endPort, int timeoutMs, int maxThreads, bool wellKnownOnly, bool showAll, string? outputPath) ParseArguments(string[] args)
    {
        const int defaultTimeout = 300;
        const int defaultThreads = 50;

    if (args.Length == 0)
        return (null, null, null, defaultTimeout, defaultThreads, false, false, null);

    var first = args[0].ToLowerInvariant();
    if (first is "--help" or "-h" or "/?" or "-?")
    {
        PrintHelp();
        Environment.Exit(0);
    }

    var host = args.ElementAtOrDefault(0);
    var timeoutMs = defaultTimeout;
    var maxThreads = defaultThreads;
    var wellKnownOnly = false;
    var showAll = false;
    string? outputPath = null;

    int? startPort = null;
    int? endPort = null;

    for (var i = 1; i < args.Length; i++)
    {
        switch (args[i].ToLowerInvariant())
        {
            case "--common":
            case "-c":
                wellKnownOnly = true;
                break;
            case "--timeout":
            case "-t":
                if (i + 1 < args.Length && int.TryParse(args[++i], out var t) && t > 0)
                    timeoutMs = t;
                break;
            case "--threads":
            case "-j":
                if (i + 1 < args.Length && int.TryParse(args[++i], out var j) && j > 0)
                    maxThreads = j;
                break;
            case "--all":
            case "-a":
                showAll = true;
                break;
            case "--output":
            case "-o":
                if (i + 1 < args.Length)
                    outputPath = args[++i];
                break;
            default:
                if (int.TryParse(args[i], out var p))
                {
                    if (startPort is null) startPort = p;
                    else endPort = p;
                }
                break;
        }
    }

    if (wellKnownOnly)
    {
        startPort = 0;
        endPort = 0;
    }
    else if (startPort is null || endPort is null)
    {
        startPort ??= 1;
        endPort ??= 1024;
    }

    if (startPort > endPort)
        (startPort, endPort) = (endPort, startPort);

    return (host, startPort, endPort, timeoutMs, maxThreads, wellKnownOnly, showAll, outputPath);
}

static void PrintHelp()
{
    Console.WriteLine();
    Console.WriteLine("PortScanner – TCP-Ports eines Hosts prüfen");
    Console.WriteLine();
    Console.WriteLine("Verwendung:");
    Console.WriteLine("  PortScanner                    Interaktiver Modus (Eingabeaufforderungen)");
    Console.WriteLine("  PortScanner <Host> [Start] [Ende] [Optionen]");
    Console.WriteLine();
    Console.WriteLine("Beispiele:");
    Console.WriteLine("  PortScanner 192.168.1.1");
    Console.WriteLine("  PortScanner 192.168.1.1 1 1024");
    Console.WriteLine("  PortScanner example.com 80 443 --timeout 500");
    Console.WriteLine("  PortScanner 192.168.1.1 --common");
    Console.WriteLine("  PortScanner 192.168.1.1 -o results.json");
    Console.WriteLine("  PortScanner 192.168.1.1 1 1024 --output results.csv");
    Console.WriteLine();
    Console.WriteLine("Optionen:");
    Console.WriteLine("  -c, --common     Nur bekannte Ports (HTTP, SSH, FTP, MySQL, …)");
    Console.WriteLine("  -t, --timeout N  Timeout pro Port in ms (Standard: 300)");
    Console.WriteLine("  -j, --threads N  Max. parallele Verbindungen (Standard: 50)");
    Console.WriteLine("  -a, --all        Auch geschlossene/gefilterte Ports anzeigen");
    Console.WriteLine("  -o, --output F   Ergebnisse in Datei speichern (.json, .csv oder .txt)");
    Console.WriteLine("  -h, --help       Diese Hilfe anzeigen");
    Console.WriteLine();
}

static async Task RunInteractiveModeAsync()
{
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║              PortScanner – Interaktiver Modus            ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("Geben Sie die gewünschten Werte ein (Enter = Standard).");
    Console.WriteLine();

    Console.Write("Host oder IP-Adresse: ");
    var hostInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(hostInput))
    {
        ConsoleHelper.WriteError("Host ist erforderlich.");
        return;
    }

    var resolved = await PortScannerService.ResolveHostAsync(hostInput);
    if (resolved is null)
    {
        ConsoleHelper.WriteError($"Host '{hostInput}' konnte nicht aufgelöst werden.");
        return;
    }

    Console.Write("Portbereich Start (Standard: 1): ");
    var startStr = Console.ReadLine()?.Trim();
    var startPort = int.TryParse(startStr, out var s) && s >= 0 && s <= 65535 ? s : 1;

    Console.Write("Portbereich Ende (Standard: 1024): ");
    var endStr = Console.ReadLine()?.Trim();
    var endPort = int.TryParse(endStr, out var e) && e >= 0 && e <= 65535 ? e : 1024;

    if (startPort > endPort)
        (startPort, endPort) = (endPort, startPort);

    Console.Write("Nur bekannte Ports scannen? (HTTP, SSH, FTP, …) [j/N]: ");
    var commonInput = Console.ReadLine()?.Trim().ToUpperInvariant();
    var wellKnownOnly = commonInput is "J" or "JA" or "Y" or "YES";

    if (wellKnownOnly)
    {
        startPort = 0;
        endPort = 0;
    }

    Console.Write("Timeout pro Port in ms (Standard: 300): ");
    var timeoutStr = Console.ReadLine()?.Trim();
    var timeoutMs = int.TryParse(timeoutStr, out var to) && to > 0 ? to : 300;

    Console.Write("Max. parallele Verbindungen (Standard: 50): ");
    var threadsStr = Console.ReadLine()?.Trim();
    var maxThreads = int.TryParse(threadsStr, out var th) && th > 0 ? th : 50;

    Console.Write("Alle Ports anzeigen (auch geschlossen/gefiltert)? [j/N]: ");
    var allInput = Console.ReadLine()?.Trim().ToUpperInvariant();
    var showAll = allInput is "J" or "JA" or "Y" or "YES";

    Console.Write("Ergebnisse in Datei speichern? (z.B. results.txt, results.json, results.csv): ");
    var outputPathInteractive = Console.ReadLine()?.Trim();

    var options = new ScanOptions
    {
        Host = resolved,
        StartPort = startPort,
        EndPort = endPort,
        TimeoutMs = timeoutMs,
        MaxThreads = maxThreads,
        UseWellKnownPortsOnly = wellKnownOnly
    };

    ConsoleHelper.WriteHeader(hostInput, options.StartPort, options.EndPort, wellKnownOnly);

    var lockObj = new object();
    var scanner = new PortScannerService(maxThreads, result =>
    {
        lock (lockObj)
            ConsoleHelper.WriteResult(result, showAll);
    });

    var results = await scanner.ScanAsync(options);
    var openCount = results.Count(r => r.IsOpen);
    var totalScanned = results.Count;

    string? osGuessInteractive = null;
    try
    {
        var (_, ttl, guess) = await OsDetectionHelper.GetTtlFromPingCommandAsync(resolved);
        if (guess != null) osGuessInteractive = ttl.HasValue ? $"{guess} (TTL={ttl})" : guess;
    }
    catch { }

    ConsoleHelper.WriteSummary(openCount, totalScanned, scanner.Elapsed, hostInput, osGuessInteractive);

    if (!string.IsNullOrWhiteSpace(outputPathInteractive))
    {
        OutputExportHelper.Export(results, outputPathInteractive, hostInput, scanner.Elapsed);
        ConsoleHelper.WriteInfo($"Ergebnisse gespeichert: {Path.GetFullPath(outputPathInteractive)}");
    }

    Console.WriteLine("Drücken Sie eine Taste zum Beenden.");
    Console.ReadKey(true);
}
