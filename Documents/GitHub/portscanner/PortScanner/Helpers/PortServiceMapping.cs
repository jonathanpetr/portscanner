namespace PortScanner.Helpers;

/// <summary>
/// Maps well-known port numbers to their service names.
/// </summary>
public static class PortServiceMapping
{
    private static readonly Dictionary<int, string> ServiceMap = new()
    {
        { 7, "Echo" },
        { 9, "Discard" },
        { 20, "FTP-Data" },
        { 21, "FTP" },
        { 22, "SSH" },
        { 23, "Telnet" },
        { 25, "SMTP" },
        { 53, "DNS" },
        { 67, "DHCP" },
        { 68, "DHCP" },
        { 69, "TFTP" },
        { 79, "Finger" },
        { 80, "HTTP" },
        { 88, "Kerberos" },
        { 110, "POP3" },
        { 111, "RPCbind" },
        { 123, "NTP" },
        { 135, "MSRPC" },
        { 137, "NetBIOS-NS" },
        { 138, "NetBIOS-DGM" },
        { 139, "NetBIOS-SSN" },
        { 143, "IMAP" },
        { 161, "SNMP" },
        { 162, "SNMP-Trap" },
        { 389, "LDAP" },
        { 443, "HTTPS" },
        { 445, "SMB" },
        { 464, "Kerberos-PW" },
        { 465, "SMTPS" },
        { 500, "IKE" },
        { 514, "Syslog" },
        { 515, "LPD" },
        { 587, "SMTP-Submission" },
        { 636, "LDAPS" },
        { 993, "IMAPS" },
        { 995, "POP3S" },
        { 1025, "MSRPC" },
        { 1433, "MSSQL" },
        { 1521, "Oracle" },
        { 2049, "NFS" },
        { 2375, "Docker" },
        { 2376, "Docker-TLS" },
        { 3000, "Dev-Server" },
        { 3306, "MySQL" },
        { 3389, "RDP" },
        { 4443, "HTTPS-Alt" },
        { 5000, "UPnP" },
        { 5432, "PostgreSQL" },
        { 5433, "PostgreSQL-Alt" },
        { 5672, "RabbitMQ" },
        { 5900, "VNC" },
        { 5984, "CouchDB" },
        { 6379, "Redis" },
        { 6443, "Kubernetes" },
        { 8000, "HTTP-Alt" },
        { 8008, "HTTP-Alt" },
        { 8080, "HTTP-Proxy" },
        { 8443, "HTTPS-Alt" },
        { 8888, "HTTP-Alt" },
        { 9000, "SonarQube" },
        { 9042, "Cassandra" },
        { 9200, "Elasticsearch" },
        { 9300, "Elasticsearch-Transport" },
        { 11211, "Memcached" },
        { 27017, "MongoDB" },
        { 27018, "MongoDB-Shard" },
        { 50000, "SAP" }
    };

    /// <summary>
    /// Well-known ports to scan when using "common ports" mode.
    /// </summary>
    public static readonly IReadOnlyList<int> WellKnownPorts = new[]
    {
        21, 22, 23, 25, 53, 80, 110, 123, 143, 161, 389, 443, 445, 3306, 3389, 5432, 5672, 5900, 6379,
        6443, 8000, 8080, 8443, 9200, 27017
    };

    public static string GetServiceName(int port)
    {
        return ServiceMap.TryGetValue(port, out var name) ? name : $"Port-{port}";
    }

    public static IReadOnlyDictionary<int, string> GetAll() => ServiceMap;
}
