namespace TradeReconciliation.Core.Services;

public interface IReconciliationService
{
    Task<ReconciliationSummary> RunAsync(DateTime tradeDate, CancellationToken cancellationToken = default);
}