using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Data;

public class DataSeeder
{
    public static void Seed(TradeReconciliationDbContext db)
    {
        if (db.Trades.Any()) return;

        var account1 = new Account { AccountNumber = "ACC-001", Name = "Demo Client A" };
        var account2 = new Account { AccountNumber = "ACC-002", Name = "Demo Client B" };
        db.Accounts.AddRange(account1, account2);
        db.SaveChanges();
        
        var tradeDate = new DateTime(2026, 4, 30, 14, 30, 0, DateTimeKind.Utc);

        var internalTrades = new[]
        {
            new Trade
            {
                TradeIdentifier = "T-001", Source = TradeSource.Internal, Symbol = "EUR/USD",
                Side = TradeSide.Buy, Quantity = 1_000_000m, Price = 1.0850m, TradeDate = tradeDate,
                Counterparty = "Bank A", AccountId = account1.Id
            },
            new Trade
            {
                TradeIdentifier = "T-002", Source = TradeSource.Internal, Symbol = "GBP/USD",
                Side = TradeSide.Sell, Quantity = 500_000m, Price = 1.2650m, TradeDate = tradeDate,
                Counterparty = "Bank B", AccountId = account1.Id
            },
            new Trade
            {
                TradeIdentifier = "T-003", Source = TradeSource.Internal, Symbol = "USD/JPY",
                Side = TradeSide.Buy, Quantity = 250_000m, Price = 154.20m, TradeDate = tradeDate,
                Counterparty = "Bank C", AccountId = account2.Id
            },
            new Trade
            {
                TradeIdentifier =  "T-004", Source = TradeSource.Internal, Symbol = "EUR/USD",
                Side = TradeSide.Sell, Quantity = 750_000m, Price = 1.0860m, TradeDate = tradeDate,
                Counterparty = "Bank A",  AccountId = account2.Id
            },
        };

        var externalTrades = new[]
        {
            new Trade
            {
                TradeIdentifier = "T-001", Source = TradeSource.External, Symbol = "EUR/USD",
                Side = TradeSide.Buy, Quantity = 1_000_000m, Price = 1.0850m, TradeDate = tradeDate,
                Counterparty = "Bank A", AccountId = account1.Id
            },
            new Trade
            {
                TradeIdentifier =  "T-002", Source = TradeSource.External, Symbol = "GBP/USD",
                Side = TradeSide.Sell, Quantity = 500_000m, Price = 1.2655m, TradeDate = tradeDate,
                Counterparty = "Bank B", AccountId = account1.Id
            },
            new Trade
            {
                TradeIdentifier =   "T-003", Source = TradeSource.External, Symbol = "USD/JPY",
                Side = TradeSide.Buy, Quantity = 250_000m, Price = 154.20m, TradeDate = tradeDate,
                Counterparty = "Bank C", AccountId = account2.Id
            },
            new Trade
            {
                TradeIdentifier = "T-005", Source = TradeSource.External, Symbol = "AUD/USD",
                Side = TradeSide.Buy, Quantity = 250_000m, Price = 0.6520m, TradeDate = tradeDate,
                Counterparty = "Bank D", AccountId = account1.Id
            },
        };
        
        db.Trades.AddRange(internalTrades);
        db.Trades.AddRange(externalTrades);
        db.SaveChanges();
    }
}