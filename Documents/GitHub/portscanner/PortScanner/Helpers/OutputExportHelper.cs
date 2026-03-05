using System.Text;
using System.Text.Json;
using PortScanner.Models;

namespace PortScanner.Helpers;

public static class OutputExportHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void Export(IReadOnlyList<ScanResult> results, string filePath, string host, TimeSpan duration)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var content = extension switch
        {
            ".json" => ExportJson(results),
            ".csv" => ExportCsv(results),
            _ => ExportText(results, host, duration)
        };
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }

    private static string ExportJson(IReadOnlyList<ScanResult> results)
    {
        var dto = results.Select(r => new
        {
            r.Host,
            r.Port,
            r.IsOpen,
            r.ServiceName,
            r.ResponseTimeMs,
            r.IsFiltered
        }).ToList();
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    private static string ExportCsv(IReadOnlyList<ScanResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Host,Port,IsOpen,ServiceName,ResponseTimeMs,IsFiltered");
        foreach (var r in results)
        {
            var status = r.IsOpen ? "Open" : r.IsFiltered ? "Filtered" : "Closed";
            sb.AppendLine($"{EscapeCsv(r.Host)},{r.Port},{status},{EscapeCsv(r.ServiceName)},{r.ResponseTimeMs?.ToString() ?? ""},{r.IsFiltered}");
        }
        return sb.ToString();
    }

    private static string ExportText(IReadOnlyList<ScanResult> results, string host, TimeSpan duration)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Port scan: {host}");
        sb.AppendLine($"Duration: {duration.TotalSeconds:F1}s");
        sb.AppendLine();
        sb.AppendLine("Port    Status    Service          ResponseTime");
        sb.AppendLine("----    ------    -------          ------------");
        foreach (var r in results)
        {
            var status = r.IsOpen ? "OPEN" : r.IsFiltered ? "FILTERED" : "CLOSED";
            var rt = r.ResponseTimeMs.HasValue ? $"{r.ResponseTimeMs}ms" : "-";
            sb.AppendLine($"{r.Port,-7} {status,-8} {r.ServiceName,-16} {rt}");
        }
        sb.AppendLine();
        sb.AppendLine($"Total: {results.Count(r => r.IsOpen)} open, {results.Count} scanned");
        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
