using System.Net;
using cidrcalculator.domain;

public class IpCalculatorTests
{
    [Fact]
    public void ParseCidr_ValidInput_ReturnsCorrectIpAndPrefix()
    {
        var (ip, prefix) = InvokeParseCidr("192.168.1.0/24");

        Assert.Equal(IPAddress.Parse("192.168.1.0"), ip);
        Assert.Equal(24, prefix);
    }

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0", "192.168.1.255", "192.168.1.1", "192.168.1.254", 256, 254)]
    [InlineData("10.0.0.0/30", "10.0.0.0", "10.0.0.3", "10.0.0.1", "10.0.0.2", 4, 2)]
    [InlineData("10.0.0.0/32", "10.0.0.0", "10.0.0.0", null, null, 1, 1)]
    [InlineData("10.0.0.0/31", "10.0.0.0", "10.0.0.1", null, null, 2, 2)]
    [InlineData("0.0.0.0/0", "0.0.0.0", "255.255.255.255", null, null, 4294967296, 4294967296)] // Hele IPv4 ruimte
    [InlineData("255.255.255.255/32", "255.255.255.255", "255.255.255.255", null, null, 1, 1)] // Enkele IP
    [InlineData("10.10.10.0/31", "10.10.10.0", "10.10.10.1", null, null, 2, 2)] // Alleen twee bruikbare (zonder broadcast concept)
    [InlineData("172.16.5.128/25", "172.16.5.128", "172.16.5.255", "172.16.5.129", "172.16.5.254", 128, 126)]
    public void GetRangeInfo_ReturnsExpectedValues(
        string cidr, string expectedNetwork, string expectedBroadcast,
        string expectedFirstHost, string expectedLastHost, int total, int usable)
    {
        var result = IpCalculator.GetRangeInfo(cidr);

        Assert.Equal(expectedNetwork, result.Network);
        Assert.Equal(expectedBroadcast, result.Broadcast);
        Assert.Equal(expectedFirstHost, result.FirstHost);
        Assert.Equal(expectedLastHost, result.LastHost);
        Assert.Equal(total, result.Total);
        Assert.Equal(usable, result.Usable);
        Assert.Equal(total, result.Addresses.Count);
    }

    [Theory]
    [InlineData("192.168.1.0")]
    [InlineData("10.0.0.0/abc")]
    public void ParseCidr_InvalidInput_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => InvokeParseCidr(input));
    }

    [Fact]
    public void RangesOverlap_WhenOverlapping_ReturnsTrue()
    {
        var range1 = IpCalculator.GetRangeInfo("192.168.1.0/24");
        var range2 = IpCalculator.GetRangeInfo("192.168.1.128/25");

        var overlap = range1.Addresses.Intersect(range2.Addresses).Any();
        Assert.True(overlap);
    }


    /// <summary>
    /// Test: Overlap tussen ranges
    /// </summary>
    [Fact]
    public void RangesOverlap_WhenDisjoint_ReturnsFalse()
    {
        var range1 = IpCalculator.GetRangeInfo("192.168.1.0/25");
        var range2 = IpCalculator.GetRangeInfo("192.168.1.128/25");

        var overlap = range1.Addresses.Intersect(range2.Addresses).Any();
        Assert.False(overlap);
    }

    /// <summary>
    /// Test: IP-range over subnet-grenzen heen (foutscenario)
    /// </summary>
    /// <param name="cidr"></param>
    [Theory]
    [InlineData("10.0.0.0/33")]
    [InlineData("10.0.0.0/-1")]
    [InlineData("999.999.999.999/24")]
    public void InvalidCidrNotation_Throws(string cidr)
    {
        Assert.ThrowsAny<Exception>(() => IpCalculator.GetRangeInfo(cidr));
    }

    [Theory]
    [InlineData("10.26.0.0/20", "10.26.16.0/20")]
    [InlineData("10.0.0.0/24", "10.0.1.0/24")]
    [InlineData("192.168.0.0/16", "192.169.0.0/16")]
    [InlineData("172.16.0.0/12", "172.32.0.0/12")]
    [InlineData("10.0.255.0/24", "10.1.0.0/24")] //Volgend octet
    [InlineData("192.168.255.0/24", "192.169.0.0/24")] //Volgend octet
    [InlineData("10.255.0.0/16", "11.0.0.0/16")] //Overgang klasse B netwerk
    public void FindNextAvailableRange(string currentRange, string expectedRange)
    {
        //var usedCidrs = IpCalculator.GetRangeInfo("10.26.16.0/20").Addresses;
        var nextRange = IpCalculator.GetNextCidrBlock(currentRange);

        Assert.Equal(expectedRange, nextRange);
    }

    // Helper to access internal method for testing
    private static (IPAddress ip, int prefix) InvokeParseCidr(string cidr)
    {
        var method = typeof(IpCalculator).GetMethod("ParseCidr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return ((IPAddress, int))method.Invoke(null, new object[] { cidr });
    }
}
