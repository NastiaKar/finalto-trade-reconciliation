using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Services;

public class ExactMatchStrategy : IMatchingStrategy
{
    public MatchStatus Compare(Trade internalTrade, Trade externalTrade, out string? discrepancyDetails)
    {
        var differences = new List<string>();
        
        if (internalTrade.Symbol != externalTrade.Symbol)
            differences.Add($"Symbol: internal={internalTrade.Symbol}, external={externalTrade.Symbol}");
        
        if (internalTrade.Side != externalTrade.Side)
            differences.Add($"Side: internal={internalTrade.Side}, external={externalTrade.Side}");

        if (internalTrade.Quantity != externalTrade.Quantity)
            differences.Add($"Quantity: internal={internalTrade.Quantity}, external={externalTrade.Quantity}");
        
        if (internalTrade.Price != externalTrade.Price)
            differences.Add($"Price: internal={internalTrade.Price}, external={externalTrade.Price}");
        
        if (internalTrade.Counterparty != externalTrade.Counterparty)
            differences.Add($"Counterparty: internal={internalTrade.Counterparty}, external={externalTrade.Counterparty}");

        if (differences.Count > 0)
        {
            discrepancyDetails = string.Join("; ", differences);
            return MatchStatus.Mismatched;
        }
        
        discrepancyDetails = null;
        return MatchStatus.Matched;
    }
}