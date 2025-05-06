using System;
using cidrcalculator.domain;
using cidrcalculator.domain.DTO;
using cidrcalculator.web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace cidrcalculator.web.Controllers;

public class IPInfoController : BaseController
{
    [HttpPost("ipinfo")]
    public IActionResult GetIpRangeInfo(IpRangeRequest request)
    {
        try
        {
            var ipRangeInfo = IpCalculator.GetRangeInfo(request.CIDR);
            
            return Ok(ipRangeInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}