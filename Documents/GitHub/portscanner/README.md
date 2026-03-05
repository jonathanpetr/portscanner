# PortScanner

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)

A .NET 8 console application that scans a host (IP or hostname) for open TCP ports. Results are shown in real time with color-coded output, service names, optional OS guess (TTL), and export to JSON/CSV/TXT.

---

## What does a port scanner do?

Computers offer **services** (e.g. web server, SSH, database) on specific **ports**. A port is a number (1–65535) that identifies which program should receive network traffic.

A **port scanner** tries to connect to a list of ports on a target machine:

- **Open** – something is listening; the connection succeeds.
- **Closed** – nothing is listening; the connection is refused.
- **Filtered** – no answer (e.g. firewall or timeout).

This helps you see which services are reachable on a host (e.g. your own server or lab machine).

---

## How it works (TCP handshake)

Port scanning uses the same mechanism as opening a normal connection:

1. **SYN** – Your machine sends a “connection request” (SYN packet) to the target IP and port.
2. **SYN-ACK** – If a service is listening, the target responds with “acknowledged” (SYN-ACK).
3. **ACK** – Your machine sends a final ACK; the connection is established.

- If you get **SYN-ACK**, the port is **open** (something is listening).
- If you get **RST** (reset), the port is **closed** (nothing listening).
- If you get **no reply** (timeout), the port is **filtered** (e.g. firewall drops the packet).

This tool uses .NET’s `TcpClient`: a successful `ConnectAsync` means the port is open; connection refused or timeout means closed or filtered.

---

## Build and run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
cd PortScanner
dotnet build
```

### Run

**Command line (with arguments):**

```bash
dotnet run -- <host> [startPort] [endPort] [options]
```

**Interactive mode (no arguments):**

```bash
dotnet run
```

You will be prompted for host, port range, timeout, output file, and other options.

---

## Usage examples

### 1. Scan port range (default 1–1024)

```bash
PortScanner.exe 192.168.1.1
PortScanner.exe 192.168.1.1 1 1024
```

### 2. Scan custom range

```bash
PortScanner.exe 192.168.1.1 80 443
PortScanner.exe example.com 1 1000
```

### 3. Scan only well-known ports

```bash
PortScanner.exe 192.168.1.1 --common
PortScanner.exe 192.168.1.1 -c
```

### 4. Save results to file (JSON, CSV, or TXT)

```bash
PortScanner.exe 192.168.1.1 -o results.txt
PortScanner.exe 192.168.1.1 --output results.json
PortScanner.exe 192.168.1.1 1 1024 --output results.csv
```

Format is chosen by file extension: `.json` (array of scan results), `.csv` (headers + rows), `.txt` (human-readable summary).

### 5. Optional parameters

| Option        | Short | Description                    | Default |
|---------------|-------|--------------------------------|--------|
| `--timeout`   | `-t`  | Timeout per port in ms         | 300    |
| `--threads`   | `-j`  | Max parallel connections       | 50     |
| `--all`       | `-a`  | Show closed/filtered ports too | off    |
| `--output`    | `-o`  | Save results to file (.json/.csv/.txt) | —  |

Examples:

```bash
PortScanner.exe 192.168.1.1 1 1024 --timeout 500 --threads 100
PortScanner.exe 192.168.1.1 --common --all -o scan.json
```

---

## Screenshots / Example output

| Color  | Status    | Meaning                          |
|--------|-----------|----------------------------------|
| Green  | `[OPEN]`  | Port is open; service is listening |
| Red    | `[CLOSED]`| Port closed (connection refused) |
| Yellow | `[FILTERED]` | No reply (timeout/firewall)   |

Example console output:

```
Scanning 192.168.1.1 (ports 1-1024)...

[OPEN]     22   → SSH (45ms)
[OPEN]     80   → HTTP (12ms)
[OPEN]    443  → HTTPS (15ms)
[CLOSED]  8080 → HTTP-Proxy
---------------------------
Scan complete: 3 open port(s) found (of 1024 scanned) in 2.3s
Host: 192.168.1.1
OS guess (TTL): Windows (TTL=128)
```

If you use `--output results.json`, you get a JSON array of objects with `host`, `port`, `isOpen`, `serviceName`, `responseTimeMs`, `isFiltered`. With `--output results.csv` you get a CSV with headers; with `--output results.txt` you get a plain-text report.

---

## OS detection (TTL fingerprinting)

The tool can guess the target OS from the **TTL** (Time To Live) in the ping response:

- **TTL ~64** → Linux/Unix (or similar)
- **TTL ~128** → Windows
- **TTL ~255** → Network device / router

This is best-effort and can be wrong (TTL can be changed or reduced by intermediate hops). The guess is shown in the summary when available.

---

## Project structure

```
PortScanner/
├── PortScanner.sln
└── PortScanner/
    ├── PortScanner.csproj
    ├── Program.cs
    ├── Models/
    │   ├── ScanResult.cs
    │   └── ScanOptions.cs
    ├── Services/
    │   └── PortScannerService.cs
    └── Helpers/
        ├── PortServiceMapping.cs
        ├── ConsoleHelper.cs
        ├── OutputExportHelper.cs
        └── OsDetectionHelper.cs
```

---

## Legal and ethical notice

> **⚠️ Use this tool only on systems you own or have explicit permission to scan.**
>
> - Unauthorized port scanning can be **illegal** and may violate computer misuse laws.
> - Many networks and providers treat port scanning as suspicious or abusive and may block or report it.
> - Always get **written permission** before scanning systems you do not control (e.g. employer or customer networks, third-party hosts).
>
> The authors are not responsible for misuse of this software.

---

## License

Use and modify as needed; ensure your use complies with applicable laws and policies.
