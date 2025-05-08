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
        
        return new IpRangeResponse
        {
            Network = UintToIp(network),
            Broadcast = UintToIp(broadcast),
            FirstHost = prefix >= 31 ? null : UintToIp(network + 1),
            LastHost = prefix >= 31 ? null : UintToIp(broadcast - 1),
            Total = addresses.Count,
            Usable = prefix >= 31 ? addresses.Count : addresses.Count - 2,
            NextAvailableCIDRRange = GetNextCidrBlock(cidr),
            Addresses = addresses
        };
    }

    public static (IPAddress baseIp, int prefix) ParseCidr(string cidr)
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

    private static string UintToIp(uint value)
    {
        return new IPAddress(BitConverter.GetBytes(value).Reverse().ToArray()).ToString();
    }

    private static uint IpToUint(IPAddress ip)
    {
        return BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
    }

    public static string GetNextCidrBlock(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("Ongeldige CIDR-notatie");

        var baseIp = IPAddress.Parse(parts[0]);
        int prefix = int.Parse(parts[1]);

        uint ipAsInt = IpToUint(baseIp);
        uint blockSize = 1u << (32 - prefix);
        uint nextStartIp = ipAsInt + blockSize;

        var nextIp = UintToIp(nextStartIp);
        return $"{nextIp}/{prefix}";
    }
}
