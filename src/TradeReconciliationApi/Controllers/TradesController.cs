using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Domain;
using TradeReconciliation.Core.Services;

namespace TradeReconciliationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradesController : ControllerBase
{
    private readonly TradeReconciliationDbContext _db;
    private readonly ITradeImporter _importer;

    public TradesController(TradeReconciliationDbContext db, ITradeImporter importer)
    {
        _db = db;
        _importer = importer;
    }

    /// <summary>
    /// List trades with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Trade>>> List(
        [FromQuery] DateTime? tradeDate,
        [FromQuery] TradeSource? source,
        [FromQuery] int? accountId,
        CancellationToken cancellationToken)
    {
        var query = _db.Trades.AsQueryable();

        if (tradeDate.HasValue)
        {
            var date = tradeDate.Value.Date;
            query = query.Where(t => t.TradeDate >= date && t.TradeDate < date.AddDays(1));
        }

        if (source.HasValue)
            query = query.Where(t => t.Source == source.Value);
        
        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var trades = await query
            .OrderBy(t => t.TradeDate)
            .ThenBy(t => t.TradeDate)
            .ToListAsync(cancellationToken);

        return Ok(trades);
    }

    /// <summary>
    /// Get a single trade by its database id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Trade>> Get(int id, CancellationToken cancellationToken)
    {
        var trade = await _db.Trades.FindAsync(new object[] { id }, cancellationToken);
        if (trade is null) return NotFound();
        return Ok(trade);
    }

    /// <summary>
    /// Upload a CSV file of trades.
    /// Defaults to importing as External (counterparty file).
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<TradeImportResult>> Import(
        IFormFile file,
        [FromQuery] TradeSource source = TradeSource.External,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Please upload a non-empty CSV file.");

        using var stream = file.OpenReadStream();
        var result = await _importer.ImportCsvAsync(stream, source, cancellationToken);
        
        return Ok(result);
    }
}