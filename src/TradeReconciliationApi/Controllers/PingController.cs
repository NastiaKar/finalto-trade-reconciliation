using Microsoft.AspNetCore.Mvc;

namespace TradeReconciliationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }
}