using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Services;

public class ReconciliationService : IReconciliationService
{
    private readonly TradeReconciliationDbContext _db;
    private readonly IMatchingStrategy _matcher;

    public ReconciliationService(
        TradeReconciliationDbContext db,
        IMatchingStrategy matcher)
    {
        _db = db;
        _matcher = matcher;
    }
    
    public async Task<ReconciliationSummary> RunAsync(
        DateTime tradeDate, 
        CancellationToken cancellationToken = default)
    {
        var date = tradeDate.Date;
        var nextDay = date.AddDays(1);

        var trades = await _db.Trades
            .Where(t => t.TradeDate >= date && t.TradeDate < nextDay)
            .ToListAsync(cancellationToken);

        var internalByKey = trades
            .Where(t => t.Source == TradeSource.Internal)
            .ToDictionary(t => t.TradeIdentifier);

        var externalByKey = trades
            .Where(t => t.Source == TradeSource.External)
            .ToDictionary(t => t.TradeIdentifier);
        
        var allKeys = new HashSet<string>(internalByKey.Keys);
        allKeys.UnionWith(externalByKey.Keys);

        var results = new List<ReconciliationResult>();
        var now = DateTime.UtcNow;

        foreach (var key in allKeys)
        {
            var hasInternal = internalByKey.TryGetValue(key, out var interalTrade);
            var hasExternal = externalByKey.TryGetValue(key, out var externalTrade);

            MatchStatus status;
            string? discrepancy = null;

            if (hasInternal && hasExternal)
            {
                status = _matcher.Compare(interalTrade!, externalTrade!, out discrepancy);
            }
            else if (hasInternal)
            {
                status = MatchStatus.MissingExternal;
            }
            else
            {
                status = MatchStatus.MissingInternal;
            }
            
            results.Add(new ReconciliationResult
            {
                TradeIdentifier = key,
                TradeDate = date,
                Status = status,
                DiscrepancyDetails =  discrepancy,
                ReconciledAt = now,
            });
        }

        var existing = _db.ReconciliationResults.Where(r => r.TradeDate == date);
        _db.ReconciliationResults.RemoveRange(existing);
        _db.ReconciliationResults.AddRange(results);

        await _db.SaveChangesAsync(cancellationToken);

        return new ReconciliationSummary(
            TradeDate: date,
            TotalTrades: results.Count,
            Matched: results.Count(r => r.Status == MatchStatus.Matched),
            Mismatched: results.Count(r => r.Status == MatchStatus.Mismatched),
            MissingInternal: results.Count(r => r.Status == MatchStatus.MissingInternal),
            MissingExternal: results.Count(r => r.Status == MatchStatus.MissingExternal));
    }
}