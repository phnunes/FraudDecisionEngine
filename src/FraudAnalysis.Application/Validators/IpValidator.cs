using System.Net;

namespace FraudAnalysis.Application.Validators;

/// <summary>
/// Valida endereços IP (IPv4 e IPv6) e detecta IPs reservados/privados.
/// </summary>
public static class IpValidator
{
    public static bool IsValid(string ip)
    {
        return !string.IsNullOrWhiteSpace(ip) &&
               IPAddress.TryParse(ip.Trim(), out _);
    }

    /// <summary>
    /// Retorna true se o IP for de uma rede privada ou reservada.
    /// Ranges privados: 10.x, 172.16-31.x, 192.168.x, loopback, link-local.
    /// </summary>
    public static bool IsPrivate(string ip)
    {
        if (!IPAddress.TryParse(ip.Trim(), out var address))
            return false;

        var bytes = address.GetAddressBytes();

        // IPv4
        if (bytes.Length == 4)
        {
            return bytes[0] == 10 ||
                   bytes[0] == 127 ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168) ||
                   (bytes[0] == 169 && bytes[1] == 254); // link-local
        }

        // IPv6 loopback (::1)
        if (bytes.Length == 16)
            return address.Equals(IPAddress.IPv6Loopback);

        return false;
    }
}
