using FluentAssertions;
using Microsoft.Identity.Client;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Domain;
using TradeReconciliation.Core.Services;
using TradeReconciliation.Tests.Helpers;

namespace TradeReconciliation.Tests.Services;

public class ReconciliationServiceTests : IDisposable
{
    private readonly TradeReconciliationDbContext _db;
    private readonly ReconciliationService _service;

    public ReconciliationServiceTests()
    {
        _db = TestDbContextFactory.Create();
        _service = new ReconciliationService(_db, new ExactMatchStrategy());
    }
    
    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task RunAsync_WithMatchingInternalAndExternal_ReturnsMatched()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", TradeSource.Internal, accountId: account.Id));
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", TradeSource.External, accountId: account.Id));

        var summary = await _service.RunAsync(TestDataFactory.DefaultTradeDate);
        
        summary.Matched.Should().Be(1);
        summary.Mismatched.Should().Be(0);
        summary.MissingInternal.Should().Be(0);
        summary.MissingExternal.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_WithDifferentPrices_ReturnsMismatched()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", source: TradeSource.Internal, accountId: account.Id, price: 1.0850m));
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", source: TradeSource.External, accountId: account.Id, price: 1.0855m));
        
        var summary = await _service.RunAsync(TestDataFactory.DefaultTradeDate);
        
        summary.Matched.Should().Be(0);
        summary.Mismatched.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_OnlyInternalTrade_ReturnsMissingExternal()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", source: TradeSource.Internal, accountId: account.Id));
        
        var summary = await _service.RunAsync(TestDataFactory.DefaultTradeDate);
        
        summary.MissingInternal.Should().Be(0);
        summary.MissingExternal.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_OnlyExternalTrade_ReturnsMissingInternal()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", source: TradeSource.External, accountId: account.Id));
        
        var summary = await _service.RunAsync(TestDataFactory.DefaultTradeDate);
        
        summary.MissingExternal.Should().Be(0);
        summary.MissingInternal.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_RunningTwice_DoesNotCreateDuplicateResults()
    {
        var account = await SeedAccountAsync();
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", source: TradeSource.Internal, accountId: account.Id));
        await SeedTradeAsync(TestDataFactory.MakeTrade(
            tradeIdentifier: "T-001", source: TradeSource.External, accountId: account.Id));
        
        await _service.RunAsync(TestDataFactory.DefaultTradeDate);
        await _service.RunAsync(TestDataFactory.DefaultTradeDate);
        
        _db.ReconciliationResults.Count().Should().Be(1);
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
}