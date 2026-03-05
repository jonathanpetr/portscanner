# PortScanner v1.0.0 — Initial Release

Self-contained Windows x64 executable. No .NET runtime required.

## Features

- **Scanning**
  - [x] Scan single IP or hostname for a port range (e.g. 1–1024)
  - [x] Scan well-known ports only (HTTP, HTTPS, SSH, FTP, SMTP, DNS, RDP, MySQL, PostgreSQL, MongoDB, etc.)
  - [x] CLI: `PortScanner.exe <host> [startPort] [endPort]` and interactive mode when no args

- **Performance & UX**
  - [x] Async/parallel scanning with configurable thread count (`--threads`)
  - [x] Configurable timeout per port (default 300 ms, `--timeout`)
  - [x] Real-time results as ports are discovered
  - [x] Color-coded output: green = open, red = closed, yellow = filtered
  - [x] Service name next to each port (e.g. 22 → SSH)
  - [x] Summary: open count, scan duration, host

- **Export & data**
  - [x] `--output` / `-o` to save results to file
  - [x] JSON (array of scan results), CSV (with headers), or TXT (readable report) by file extension
  - [x] 50+ well-known port names (Redis, RabbitMQ, Elasticsearch, Docker, Kubernetes, VNC, LDAP, SMB, NTP, SNMP, etc.)

- **OS detection**
  - [x] Basic OS fingerprinting from ping TTL (Linux ~64, Windows ~128, network device ~255)
  - [x] OS guess shown in scan summary

## Usage

```bash
PortScanner.exe 192.168.1.1
PortScanner.exe 192.168.1.1 1 1024 --common -o results.json
```

See [README](README.md) for full documentation.

**Use only on systems you own or have permission to scan.**
