namespace TradeReconciliation.Core.Services;

public interface IPnLCalculator
{
    /// <summary>
    /// Computes realized P&L and remaining open positions for the given trade date,
    /// optionally filtered by a single account.
    /// </summary>
    Task<List<AccountSymbolPnL>> CalculateAsync(
        DateTime tradeDate,
        int? accountId = null,
        CancellationToken cancellationToken = default);
}