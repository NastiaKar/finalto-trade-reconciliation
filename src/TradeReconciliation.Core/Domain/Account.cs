namespace TradeReconciliation.Core.Domain;

public class Account
{
    public int Id { get; set; }
    public required string AccountNumber { get; set; }
    public required string Name { get; set; }

    public List<Trade> Trades { get; set; } = new();
}