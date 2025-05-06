using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using cidrcalculator.domain.DTO;

namespace cidrcalculator.domain;

public static class IpCalculator
{
    public static IpRangeResponse GetRangeInfo(string cidr)
    {
        var (baseIp, prefix) = ParseCidr(cidr);
        var baseUint = IpToUint(baseIp);
        var mask = GetSubnetMask(prefix);

        var network = baseUint & mask;
        var broadcast = network | ~mask;

        var addresses = GetAllAddresses(network, broadcast, prefix);
        var result = BuildRangeInfo(network, broadcast, prefix, addresses);

        return result;
    }

    public static string? FindNextAvailableRange(string vnetCidr, List<string> usedCidrs, int desiredCidr)
    {
        var vnet = ParseCidr(vnetCidr);
        var vnetBaseIp = IpToLong(vnet.baseIp);
        var vnetEndIp = vnetBaseIp + (ulong)Math.Pow(2, 32 - vnet.prefix) - 1;

        // Zet de gebruikte CIDRs om naar numerieke waarden
        var usedNetworks = usedCidrs.Select(cidr => ParseCidr(cidr)).Select(c => IpToLong(c.baseIp)).ToList();

        ulong currentNetwork = vnetBaseIp;

        // Zoek naar het eerstvolgende beschikbare subnet
        while (currentNetwork <= vnetEndIp)
        {
            // Maak het subnet van de huidige positie
            var candidateNetwork = LongToIp(currentNetwork);
            var candidateSubnet = $"{candidateNetwork}/{desiredCidr}";

            // Controleer of dit subnet al in gebruik is
            if (!usedNetworks.Any(u => IsNetworkInUse(candidateSubnet, u, desiredCidr)))
            {
                return candidateSubnet;
            }

            // Verhoog naar het volgende subnet
            currentNetwork += (ulong)Math.Pow(2, 32 - desiredCidr);
        }

        return null; // Geen beschikbare range gevonden
    }

    private static (IPAddress baseIp, int prefix) ParseCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid CIDR notation");

        var ip = IPAddress.Parse(parts[0]);
        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            throw new NotSupportedException("Only IPv4 is supported");

        var prefix = int.Parse(parts[1]);
        return (ip, prefix);
    }

    private static uint IpToUint(IPAddress ip)
    {
        return BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
    }

    private static uint GetSubnetMask(int prefix)
    {
        return ~(uint.MaxValue >> prefix);
    }

    private static List<string> GetAllAddresses(uint network, uint broadcast, int cidrRange)
    {
        var list = new List<string>();
        for (uint i = network; i <= broadcast; i++)
        {
            list.Add($"{new IPAddress(BitConverter.GetBytes(i).Reverse().ToArray()).ToString()}/{cidrRange}");
        }
        return list;
    }

    private static IpRangeResponse BuildRangeInfo(uint network, uint broadcast, int prefix, List<string> addresses)
    {
        return new IpRangeResponse
        {
            Network = UintToIp(network),
            Broadcast = UintToIp(broadcast),
            FirstHost = prefix >= 31 ? null : UintToIp(network + 1),
            LastHost = prefix >= 31 ? null : UintToIp(broadcast - 1),
            Total = addresses.Count,
            Usable = prefix >= 31 ? addresses.Count : addresses.Count - 2,
            Addresses = addresses
        };
    }

    private static string UintToIp(uint value)
    {
        return new IPAddress(BitConverter.GetBytes(value).Reverse().ToArray()).ToString();
    }

    private static ulong IpToLong(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return (ulong)bytes[0] << 24 | (ulong)bytes[1] << 16 | (ulong)bytes[2] << 8 | bytes[3];
    }

    // Zet een numerieke waarde terug naar een IP-adres
    private static IPAddress LongToIp(ulong longIp)
    {
        return new IPAddress(new byte[]
        {
            (byte)(longIp >> 24),
            (byte)(longIp >> 16),
            (byte)(longIp >> 8),
            (byte)(longIp)
        });
    }

    private static bool IsNetworkInUse(string subnet, ulong usedNetwork, int desiredCidr)
    {
        var (baseIp, prefix) = ParseCidr(subnet);
        var network = IpToLong(baseIp);

        // Controleer of het subnet al in gebruik is door een vergelijking van de eerste bits
        var mask = (ulong)Math.Pow(2, 32 - desiredCidr) - 1;
        return (network & ~mask) == (usedNetwork & ~mask);
    }
}
