using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Services;
using TradeReconciliation.Core.Domain;

namespace TradeReconciliationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReconciliationController : ControllerBase
{
    private readonly IReconciliationService _service;
    private readonly TradeReconciliationDbContext _db;

    public ReconciliationController(
        IReconciliationService service,
        TradeReconciliationDbContext db)
    {
        _service = service;
        _db = db;
    }

    /// <summary>
    /// Runs reconciliation for the given trade date.
    /// Existing results for that date are replaced.
    /// </summary>
    [HttpPost("run")]
    public async Task<ActionResult<ReconciliationSummary>> Run(
        [FromQuery] DateTime tradeDate,
        CancellationToken cancellationToken)
    {
        var summary = await _service.RunAsync(tradeDate, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Returns reconciliation results for the given trade date,
    /// optionally filtered by status.
    /// </summary>
    [HttpGet("results")]
    public async Task<IActionResult> GetResults(
        [FromQuery] DateTime tradeDate,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = _db.ReconciliationResults
            .Where(r => r.TradeDate == tradeDate.Date);

        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<MatchStatus>(status, ignoreCase: true, out var parsed))
        {
            query = query.Where(r => r.Status == parsed);
        }

        var results = await query
            .OrderBy(r => r.Status)
            .ThenBy(r => r.TradeIdentifier)
            .ToListAsync(cancellationToken);
        
        return Ok(results);
    }
}