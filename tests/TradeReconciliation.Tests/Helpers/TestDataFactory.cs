using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Tests.Helpers;

public static class TestDataFactory
{
    public static readonly DateTime DefaultTradeDate = new(2026, 4, 30, 14, 30, 0, DateTimeKind.Utc);
    
    public static Account MakeAccount(string number = "ACC-001", string name = "Test Account") 
        => new() { AccountNumber = number, Name = name };

    public static Trade MakeTrade(
        string tradeIdentifier = "T-001",
        TradeSource source = TradeSource.Internal,
        string symbol = "EUR/USD",
        TradeSide side = TradeSide.Buy,
        decimal quantity = 1_000_000m,
        decimal price = 1.0850m,
        string counterparty = "Bank A",
        int accountId = 1,
        DateTime? tradeDate = null) => new()
    {
        TradeIdentifier = tradeIdentifier,
        Source = source,
        Symbol = symbol,
        Side = side,
        Quantity = quantity,
        Price = price,
        TradeDate = tradeDate ?? DefaultTradeDate,
        Counterparty = counterparty,
        AccountId = accountId,
    };
}