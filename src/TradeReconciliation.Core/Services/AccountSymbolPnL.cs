namespace TradeReconciliation.Core.Services;

public record AccountSymbolPnL(
    int AccountId,
    string AccountNumber,
    string Symbol,
    int TradeCount,
    decimal TotalBought,
    decimal TotalSold,
    decimal RemainingPosition,
    decimal RealizedPnL);