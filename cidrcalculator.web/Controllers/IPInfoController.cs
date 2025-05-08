using System;
using cidrcalculator.domain;
using cidrcalculator.domain.DTO;
using cidrcalculator.web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace cidrcalculator.web.Controllers;

public class IPInfoController : BaseController
{
    [HttpPost("range-info")]
    public IActionResult GetIpRangeInfo([FromBody] IpRangeRequest request)
    {
        try
        {
            var(baseIp, prefix) = IpCalculator.ParseCidr(request.CIDR);

            if (prefix < 1 || prefix > 32)
                return BadRequest("Prefix moet tussen 1 en 32 liggen.");

            var ipRangeInfo = IpCalculator.GetRangeInfo(request.CIDR);
            
            return Ok(ipRangeInfo);
        }
        catch (FormatException)
        {
            return BadRequest("IP-adresformaat is ongeldig.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Er is een onverwachte fout opgetreden.");
        }
    }
}