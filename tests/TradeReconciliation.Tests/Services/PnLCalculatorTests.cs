using FluentAssertions;
using Microsoft.Data.SqlClient;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Domain;
using TradeReconciliation.Core.Migrations;
using TradeReconciliation.Core.Services;
using TradeReconciliation.Tests.Helpers;

namespace TradeReconciliation.Tests.Services;

public class PnLCalculatorTests : IDisposable
{
    private readonly TradeReconciliationDbContext _db;
    private readonly PnLCalculator _calculator;

    public PnLCalculatorTests()
    {
        _db = TestDbContextFactory.Create();
        _calculator = new PnLCalculator(_db);
    }
    
    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CalculateAsync_WithNoTrades_ReturnsEmptyList()
    {
        var results = await _calculator.CalculateAsync(TestDataFactory.DefaultTradeDate);
        
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateAsync_WithSingleBuy_HasOpenPositionAndZeroRealizedPnL()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(MakeBuy(account.Id, 1_000_000m, 1.0850m));
        
        var results = await _calculator.CalculateAsync(TestDataFactory.DefaultTradeDate);
        
        results.Should().HaveCount(1);
        var pnl = results[0];
        pnl.RemainingPosition.Should().Be(1_000_000m);
        pnl.RealizedPnL.Should().Be(0m);
        pnl.TotalBought.Should().Be(1_000_000m);
        pnl.TotalSold.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithFullCloseOfSingleLot_RealizesExpectedPnL()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(MakeBuy(account.Id, 1_000_000m, 1.0850m, tradeDate: TestDataFactory.DefaultTradeDate));
        await SeedTradeAsync(MakeSell(account.Id, 1_000_000m, 1.0900m, tradeDate: TestDataFactory.DefaultTradeDate.AddHours(2)));
        
        var results = await _calculator.CalculateAsync(TestDataFactory.DefaultTradeDate);
        
        results.Should().HaveCount(1);
        var pnl = results[0];
        pnl.RealizedPnL.Should().Be((1.0900m - 1.0850m) * 1_000_000m);
        pnl.RemainingPosition.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithFifoMatching_ConsumesOldestFirst()
    {
        // arrange:
        // 09:00 buy 1M @ 1.0850
        // 10:00 buy 1M @ 1.0860
        // 11:00 sell 1.5M @ 1.0900
        //
        // expectation:
        // 1.0M closes against the 1.0850 lot -> (1.0900 - 1.0850) * 1.0M = 5000
        // 0.5M closes against the 1.0860 lot -> (1.0900 - 1.0860) * 0.5M = 2000
        // remaining: 0.5M @ 1.0860
        // realized: 7000
        var account = await SeedAccountAsync();
        var baseTime = TestDataFactory.DefaultTradeDate;
        await SeedTradeAsync(MakeBuy(account.Id, 1_000_000m, 1.0850m, tradeDate: baseTime));
        await SeedTradeAsync(MakeBuy(account.Id, 1_000_000m, 1.0860m, tradeDate: baseTime.AddHours(1)));
        await SeedTradeAsync(MakeSell(account.Id, 1_500_000m, 1.0900m, tradeDate: baseTime.AddHours(2)));
        
        var results = await _calculator.CalculateAsync(TestDataFactory.DefaultTradeDate);
        
        var pnl = results.Single();
        pnl.RealizedPnL.Should().Be(7000m);
        pnl.RemainingPosition.Should().Be(500_000m);
    }

    [Fact]
    public async Task CalculateAsync_FilterByAccountId_ReturnsOnlyThatAccount()
    {
        var account1 = await SeedAccountAsync("ACC-001");
        var account2 = await SeedAccountAsync("ACC-002");
        await SeedTradeAsync(MakeBuy(account1.Id, 100m, 1m));
        await SeedTradeAsync(MakeBuy(account2.Id, 200m, 2m));
        
        var results = await _calculator.CalculateAsync(TestDataFactory.DefaultTradeDate,
            accountId: account1.Id);
        
        results.Should().HaveCount(1);
        results[0].AccountId.Should().Be(account1.Id);
    }

    [Fact]
    public async Task CalculateAsync_DifferentSymbolsAreCalculatedDifferently()
    {
        var account = await SeedAccountAsync();
        var t = TestDataFactory.DefaultTradeDate;
        
        await SeedTradeAsync(MakeBuy(account.Id, 1000m, 1.10m, symbol: "EUR/USD", tradeDate: t));
        await SeedTradeAsync(MakeSell(account.Id, 1000m, 1.20m, symbol: "EUR/USD", tradeDate: t.AddHours(1)));
        
        await SeedTradeAsync(MakeBuy(account.Id, 500m, 1.25m, symbol: "GBP/USD", tradeDate: t));
        await SeedTradeAsync(MakeSell(account.Id, 500m, 1.30m, symbol: "GBP/USD", tradeDate: t.AddHours(1)));
        
        var results = await _calculator.CalculateAsync(t);
        
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.Symbol == "EUR/USD"
                                      && r.RealizedPnL == (1.20m - 1.10m) * 1000m);
        results.Should().Contain(r => r.Symbol == "GBP/USD"
                                      && r.RealizedPnL == (1.30m - 1.25m) * 500m);
    }

    private async Task<Account> SeedAccountAsync(string number = "ACC-001")
    {
        var account = TestDataFactory.MakeAccount(number);
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    private async Task SeedTradeAsync(Trade trade)
    {
        _db.Trades.Add(trade);
        await _db.SaveChangesAsync();
    }

    private static Trade MakeBuy(int accountId, decimal qty, decimal price, string symbol = "EUR/USD",
        DateTime? tradeDate = null)
        => TestDataFactory.MakeTrade(
            accountId: accountId,
            side: TradeSide.Buy,
            quantity: qty,
            price: price,
            symbol: symbol,
            tradeDate: tradeDate);
    
    private static Trade MakeSell(int accountId, decimal qty, decimal price, string symbol = "EUR/USD",
        DateTime? tradeDate = null)
        => TestDataFactory.MakeTrade(
            accountId: accountId,
            side: TradeSide.Sell,
            quantity: qty,
            price: price,
            symbol: symbol,
            tradeDate: tradeDate);
}