namespace TradeReconciliation.Core.Domain;

public enum TradeSide
{
    Buy = 1,
    Sell = 2
}

public enum TradeSource
{
    Internal = 1,
    External = 2
}

public enum MatchStatus
{
    Matched = 1, // same trade id and details
    Mismatched = 2, // same trade id, different details
    MissingInternal = 3,
    MissingExternal = 4
}