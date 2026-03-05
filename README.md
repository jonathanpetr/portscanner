# PortScanner

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)

**PortScanner** is a .NET 8 console application that scans a host (IP or hostname) for open TCP ports.  
Results are shown in real time with color-coded output, service names, optional OS detection (TTL), and export to JSON/CSV/TXT.

---

## Features

- **Scanning**
  - Scan a single host (IP or DNS name)
  - Scan a port range (e.g. `1–1024`)
  - Scan only well-known ports (HTTP, HTTPS, SSH, FTP, SMTP, DNS, RDP, MySQL, PostgreSQL, MongoDB, Redis, RabbitMQ, Elasticsearch, Docker, Kubernetes, VNC, LDAP, SMB, NTP, SNMP, …)
  - CLI: `PortScanner.exe <host> [startPort] [endPort]`
  - Interactive mode when the program is started without arguments

- **Performance & UX**
  - Asynchronous, parallel scanning with `async/await` and `SemaphoreSlim`
  - Configurable timeout per port (default: **300 ms**, `--timeout`)
  - Configurable parallelism (`--threads`)
  - **Live output** as soon as ports are detected (not only at the end)
  - Color-coded console:
    - **Green** = open
    - **Red** = closed
    - **Yellow** = filtered (timeout / firewall)
  - Service name shown via a `Dictionary<int, string>` (e.g. port 22 → SSH)
  - Summary at the end: open ports, total count, duration, host, OS guess

- **Export & data**
  - `--output` / `-o` flag to save results to a file
  - Format depends on file extension:
    - `.json` → JSON array of ScanResult objects
    - `.csv` → CSV with header row
    - `.txt` → human-readable text report
  - Includes host, port, status (open/closed/filtered), service name, response time, IsFiltered

- **OS detection (TTL fingerprinting)**
  - Runs a `ping` and reads the TTL value from the output
  - Heuristics:
    - TTL ~64 → Linux/Unix (or similar)
    - TTL ~128 → Windows
    - TTL ~255 → network device / router
  - OS guess is shown in the summary (e.g. `Windows (TTL=128)`)

---

## How does a port scanner work?

Computers offer **services** (e.g. web server, SSH, databases) on specific **ports**.  
A port is a number from `1` to `65535` that identifies which process receives the connection.

A port scanner tries to establish a TCP connection to these ports:

- **Open** – A service is listening; the connection is accepted.
- **Closed** – No service is listening; the connection is actively refused.
- **Filtered** – No response (firewall / packet dropped).

### TCP handshake in short

1. **SYN** – Your machine sends a SYN (connection request) to host:port.
2. **SYN-ACK** – The host replies with SYN-ACK if a service is listening.
3. **ACK** – Your machine sends ACK → connection is established.

- **SYN-ACK** → port is **open**.
- **RST** (reset) → port is **closed**.
- **No response** (timeout) → port is **filtered**.

PortScanner uses `TcpClient.ConnectAsync` internally:
- Successful connect → port open.
- ConnectionRefused → closed.
- Timeout / no response → filtered.
---

## Installation / What do I need to download?
### Option A: Ready-to-use .exe (recommended if you only want to scan)
1. Go to [Releases](https://github.com/jonathanpetr/portscanner/releases).
2. Under **v1.0.0**, download **PortScanner.exe** (Windows x64).
3. No further installation required – the .exe is self-contained (includes everything).
4. To run: double-click, or use the command prompt:
   ```bash
   PortScanner.exe 192.168.1.1
   PortScanner.exe 192.168.1.1 1 1024 -o results.txt
Option B: Build from source
Download and install the .NET 8 SDK:
https://dotnet.microsoft.com/download/dotnet/8.0
Clone the repository, then in the PortScanner folder run:
cd PortScanner
dotnet build
dotnet run -- 192.168.1.1
Or use the build script to produce your own .exe: .\build-release.ps1PortScanner.exe 192.168.1.1 1 1024 -o results.txt
