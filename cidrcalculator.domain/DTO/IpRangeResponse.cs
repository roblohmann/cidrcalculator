using System.Collections.Generic;

namespace cidrcalculator.domain.DTO
{
    public class IpRangeResponse
    {
        public string Network { get; set; }
        public string Broadcast { get; set; }
        public string FirstHost { get; set; }
        public string LastHost { get; set; }
        public int Total { get; set; }
        public int Usable { get; set; }
        public List<string> Addresses { get; set; }
        public string NextAvailableCIDRRange { get; set; }
    }
}
