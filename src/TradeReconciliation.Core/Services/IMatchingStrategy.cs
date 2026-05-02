using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Services;

public interface IMatchingStrategy
{
    /// <summary>
    /// Compares two trades that share the same TradeIdentifier and reports
    /// whether they match, and if not, why
    /// </summary>
    /// <param name="internalTrade">The internal book's record of the trade.</param>
    /// <param name="externalTrade">The counterparty's record of the trade.</param>
    /// <param name="discrepancyDetails">Description of any differences.</param>
    /// <returns>Matched or Mismatched.</returns>
    MatchStatus Compare(Trade internalTrade, Trade externalTrade, out string? discrepancyDetails);
}