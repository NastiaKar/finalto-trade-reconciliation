namespace TradeReconciliation.Core.Services;

public record ReconciliationSummary(
    DateTime TradeDate,
    int TotalTrades,
    int Matched,
    int Mismatched,
    int MissingInternal,
    int MissingExternal);