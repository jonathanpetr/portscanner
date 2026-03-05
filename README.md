# PortScanner

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)

**PortScanner** ist ein .NET-8-Konsolenprogramm, mit dem du einen Host (IP oder Hostname) nach offenen TCP-Ports scannen kannst.  
Das Tool zeigt Ergebnisse in Echtzeit an – farblich hervorgehoben, inklusive Dienstnamen, optionaler OS-Erkennung (TTL) und Export der Ergebnisse.

---

## Features

- **Scanning**
  - Scan eines einzelnen Hosts (IP oder DNS-Name)
  - Scan eines Portbereichs (z. B. `1–1024`)
  - Scan nur „well-known ports“ (HTTP, HTTPS, SSH, FTP, SMTP, DNS, RDP, MySQL, PostgreSQL, MongoDB, Redis, RabbitMQ, Elasticsearch, Docker, Kubernetes, VNC, LDAP, SMB, NTP, SNMP, …)
  - CLI: `PortScanner.exe <host> [startPort] [endPort]`
  - Interaktiver Modus, wenn das Programm ohne Argumente gestartet wird

- **Performance & UX**
  - Asynchrones, paralleles Scannen mit `async/await` und `SemaphoreSlim`
  - Konfigurierbarer Timeout pro Port (Standard: **300 ms**, `--timeout`)
  - Konfigurierbare Parallelität (`--threads`)
  - **Live-Ausgabe**, sobald Ports erkannt werden (nicht erst am Ende)
  - Farbige Konsole:
    - **Grün** = offen
    - **Rot** = geschlossen
    - **Gelb** = gefiltert (Timeout / Firewall)
  - Dienstname wird anhand eines `Dictionary<int, string>` angezeigt (z. B. Port 22 → SSH)
  - Zusammenfassung am Ende: offene Ports, Gesamtanzahl, Dauer, Host, OS-Vermutung

- **Export & Daten**
  - Flag `--output` / `-o` zum Speichern der Ergebnisse in einer Datei
  - Format hängt von der Dateiendung ab:
    - `.json` → JSON-Array von ScanResult-Objekten
    - `.csv` → CSV mit Kopfzeile
    - `.txt` → menschenlesbarer Textreport
  - Enthält Host, Port, Status (open/closed/filtered), ServiceName, ResponseTimeMs, IsFiltered

- **OS-Erkennung (TTL-Fingerprinting)**
  - Führt ein `ping` aus und liest den TTL-Wert aus der Ausgabe
  - Heuristik:
    - TTL ~64 → Linux/Unix (oder ähnlich)
    - TTL ~128 → Windows
    - TTL ~255 → Netzwerkgerät / Router
  - Die OS-Vermutung wird in der Zusammenfassung angezeigt (z. B. `Windows (TTL=128)`)

---

## Wie funktioniert ein Port-Scanner?

Computer bieten **Dienste** (z. B. Webserver, SSH, Datenbanken) über bestimmte **Ports** an.  
Ein Port ist eine Zahl von `1`–`65535`, die angibt, welcher Prozess die Verbindung bekommt.

Ein Port-Scanner versucht, eine TCP-Verbindung zu diesen Ports aufzubauen:

- **Open** – Ein Dienst lauscht, die Verbindung wird akzeptiert.
- **Closed** – Kein Dienst lauscht, die Verbindung wird aktiv abgelehnt.
- **Filtered** – Es kommt keine Antwort (Firewall / Paket wird verworfen).

### Kurz zum TCP-Handshake

1. **SYN** – Dein Rechner sendet ein SYN (Verbindungsanfrage) an Host:Port.
2. **SYN-ACK** – Der Host antwortet mit SYN-ACK, wenn ein Dienst lauscht.
3. **ACK** – Dein Rechner bestätigt mit ACK → Verbindung steht.

- **SYN-ACK** → Port ist **offen**.
- **RST** (Reset) → Port ist **geschlossen**.
- **Keine Antwort** (Timeout) → Port ist **gefiltert**.

Der PortScanner nutzt intern `TcpClient.ConnectAsync`:
- Erfolgreicher Connect → Port open.
- ConnectionRefused → closed.
- Timeout / keine Antwort → filtered.

---

## Projektstruktur

```text
PortScanner/
├── PortScanner.sln
└── PortScanner/
    ├── PortScanner.csproj
    ├── Program.cs                  # Entry Point, CLI + interaktiver Modus
    ├── Models/
    │   ├── ScanResult.cs           # Ergebnis eines einzelnen Ports
    │   └── ScanOptions.cs          # Scan-Konfiguration
    ├── Services/
    │   └── PortScannerService.cs   # Async/parallel Port-Scan-Logik (TcpClient + SemaphoreSlim)
    └── Helpers/
        ├── PortServiceMapping.cs   # Mapping Port → Dienstname + WellKnownPorts
        ├── ConsoleHelper.cs        # Farbige Ausgabe, Header, Summary
        ├── OutputExportHelper.cs   # Export nach JSON/CSV/TXT
        └── OsDetectionHelper.cs    # TTL-basiertes OS-Fingerprinting
