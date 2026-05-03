using Microsoft.AspNetCore.Mvc;
using TradeReconciliation.Core.Services;

namespace TradeReconciliationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PnLController : ControllerBase
{
    private readonly IPnLCalculator _calculator;

    public PnLController(IPnLCalculator calculator)
    {
        _calculator = calculator;
    }

    /// <summary>
    /// Returns realized P&L and remaining open positions for the given trade date, optionally
    /// filtered by one account.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AccountSymbolPnL>>> Get(
        [FromQuery] DateTime tradeDate,
        [FromQuery] int? accountId,
        CancellationToken cancellationToken)
    {
        var results = await _calculator.CalculateAsync(tradeDate, accountId, cancellationToken);
        return Ok(results);
    }
}