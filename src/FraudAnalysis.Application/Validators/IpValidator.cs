using System.Net;

namespace FraudAnalysis.Application.Validators;

// Valida endereços IP e detecta IPs privados/reservados.
public static class IpValidator
{
    public static bool IsValid(string ip)
    {
        return !string.IsNullOrWhiteSpace(ip) &&
               IPAddress.TryParse(ip.Trim(), out _);
    }

    // Retorna true se o IP for de rede privada (10.x, 172.16-31.x, 192.168.x, loopback, link-local).
    public static bool IsPrivate(string ip)
    {
        if (!IPAddress.TryParse(ip.Trim(), out var address))
            return false;

        var bytes = address.GetAddressBytes();

        if (bytes.Length == 4)
        {
            return bytes[0] == 10 ||
                   bytes[0] == 127 ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168) ||
                   (bytes[0] == 169 && bytes[1] == 254);
        }

        if (bytes.Length == 16)
            return address.Equals(IPAddress.IPv6Loopback);

        return false;
    }
}
