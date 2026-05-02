namespace TradeReconciliation.Core.Domain;

public class ReconciliationResult
{
    public int Id { get; set; }
    public required string TradeIdentifier { get; set; }
    public DateTime TradeDate { get; set; }
    public MatchStatus Status { get; set; }
    public string? DiscrepancyDetails { get; set; }
    public DateTime ReconciledAt { get; set; }
}