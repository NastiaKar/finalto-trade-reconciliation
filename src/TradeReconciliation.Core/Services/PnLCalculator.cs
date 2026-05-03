using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Services;

public class PnLCalculator : IPnLCalculator
{

    private readonly TradeReconciliationDbContext _db;

    public PnLCalculator(TradeReconciliationDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<AccountSymbolPnL>> CalculateAsync(
        DateTime tradeDate, 
        int? accountId = null, 
        CancellationToken cancellationToken = default)
    {
        var date = tradeDate.Date;
        var nextDay = date.AddDays(1);
        
        var query = _db.Trades
            .Include(t => t.Account)
            .Where(t => t.Source == TradeSource.Internal
            && t.TradeDate >= date
            && t.TradeDate < nextDay);
        
        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var trades = await query
            .OrderBy(t => t.TradeDate)
            .ToListAsync(cancellationToken);
        
        var groups = trades.GroupBy(t => new { t.AccountId, t.Symbol });
        var results = new List<AccountSymbolPnL>();

        foreach (var group in groups)
        {
            var pnl = ComputeGroupPnL(group.ToList());
            var firstTrade = group.First();
            
            results.Add(new AccountSymbolPnL(
                AccountId: group.Key.AccountId,
                AccountNumber: firstTrade.Account!.AccountNumber,
                Symbol: group.Key.Symbol,
                TradeCount: group.Count(),
                TotalBought: pnl.totalBought,
                TotalSold: pnl.totalSold,
                RemainingPosition: pnl.remainingPosition,
                RealizedPnL: pnl.realizedPnl));
        }
        
        return results
            .OrderBy(r => r.AccountNumber)
            .ThenBy(r => r.Symbol)
            .ToList();
    }

    /// <summary>
    /// FIFO realized-P&L matching for one (account, symbol) group of trades.
    /// </summary>
    private static (decimal totalBought, decimal totalSold,
        decimal remainingPosition, decimal realizedPnl)
        ComputeGroupPnL(List<Trade> trades)
    {
        var openLots = new List<(decimal qty, decimal price)>();
        decimal totalBought = 0m;
        decimal totalSold = 0m;
        decimal realizedPnL = 0m;

        foreach (var trade in trades) 
        {
            if (trade.Side == TradeSide.Buy)
            {
                openLots.Add((trade.Quantity, trade.Price));
                totalBought += trade.Quantity;
            }
            else
            {
                totalSold += trade.Quantity;
                var sellQtyRemaining = trade.Quantity;

                while (sellQtyRemaining > 0 && openLots.Count > 0)
                {
                    var lot = openLots[0];

                    if (lot.qty <= sellQtyRemaining)
                    {
                        realizedPnL += (trade.Price - lot.price) * lot.qty;
                        sellQtyRemaining -= lot.qty;
                        openLots.RemoveAt(0);
                    }
                    else
                    {
                        realizedPnL += (trade.Price - lot.price) * sellQtyRemaining;
                        openLots[0] = (lot.qty - sellQtyRemaining, lot.price);
                        sellQtyRemaining = 0;
                    }
                }
            }
        }

        var remainingPosition = openLots.Sum(l => l.qty);
        return (totalBought, totalSold, remainingPosition, realizedPnL);
    }
}