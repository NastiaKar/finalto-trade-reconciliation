using FluentAssertions;
using TradeReconciliation.Core.Domain;
using TradeReconciliation.Core.Services;
using TradeReconciliation.Tests.Helpers;

namespace TradeReconciliation.Tests.Services;

public class ExactMatchStrategyTests
{
    private readonly ExactMatchStrategy _strategy = new();

    [Fact]
    public void Compare_WhenAllFieldsMatch_ReturnsMatchedWithNoDiscrepancy()
    {
        var internalTrade = TestDataFactory.MakeTrade(source: TradeSource.Internal);
        var externalTrade = TestDataFactory.MakeTrade(source: TradeSource.External);

        var status = _strategy.Compare(internalTrade, externalTrade, out var discrepancy);
        
        status.Should().Be(MatchStatus.Matched);
        discrepancy.Should().BeNull();
    }

    [Fact]
    public void Compare_WhenPricesDiffer_ReturnsMismatchedAndDiscrepancyMentionsPrice()
    {
        var internalTrade = TestDataFactory.MakeTrade(source: TradeSource.Internal, price: 1.0850m);
        var externalTrade = TestDataFactory.MakeTrade(source: TradeSource.External, price: 1.0855m);
        
        var status = _strategy.Compare(internalTrade, externalTrade, out var discrepancy);
        
        status.Should().Be(MatchStatus.Mismatched);
        discrepancy.Should().NotBeNull();
        discrepancy.Should().Contain("Price");
    }

    [Fact]
    public void Compare_WhenQuantitiesDiffer_ReturnsMismatched()
    {
        var internalTrade = TestDataFactory.MakeTrade(quantity: 1_000_000m);
        var externalTrade = TestDataFactory.MakeTrade(quantity: 999_999m);
        
        var status = _strategy.Compare(internalTrade, externalTrade, out var discrepancy);
        
        status.Should().Be(MatchStatus.Mismatched);
        discrepancy.Should().Contain("Quantity");
    }

    [Fact]
    public void Compare_WhenSidesDiffer_ReturnsMismatched()
    {
        var internalTrade = TestDataFactory.MakeTrade(side: TradeSide.Buy);
        var externalTrade = TestDataFactory.MakeTrade(side: TradeSide.Sell);
        
        var status = _strategy.Compare(internalTrade, externalTrade, out var discrepancy);
        
        status.Should().Be(MatchStatus.Mismatched);
        discrepancy.Should().Contain("Side");
    }

    [Fact]
    public void Compare_WithMultipleDifferences_ReportsAllOfThemInDiscrepancy()
    {
        var internalTrade = TestDataFactory.MakeTrade(
            price: 1.0850m, quantity: 1_000_000m, counterparty: "Bank A");
        var externalTrade = TestDataFactory.MakeTrade(
            price: 1.0900m, quantity: 999_999m, counterparty: "Bank B");
        
        var status = _strategy.Compare(internalTrade, externalTrade, out var discrepancy);
        
        status.Should().Be(MatchStatus.Mismatched);
        discrepancy.Should().Contain("Price");
        discrepancy.Should().Contain("Quantity");
        discrepancy.Should().Contain("Counterparty");
    }
}