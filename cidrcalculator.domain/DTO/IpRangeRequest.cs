namespace cidrcalculator.domain.DTO;

public record IpRangeRequest
{
    public string CIDR { get; set; }
}