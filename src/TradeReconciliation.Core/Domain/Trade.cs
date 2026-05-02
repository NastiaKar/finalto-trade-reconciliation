namespace TradeReconciliation.Core.Domain;

public class Trade
{
    public int Id { get; set; }
    public required string TradeIdentifier { get; set; }
    public TradeSource Source { get; set; }
    public required string Symbol { get; set; }
    public TradeSide Side { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public DateTime TradeDate { get; set; }
    public required string Counterparty { get; set; }
    
    public int AccountId { get; set; }
    public Account? Account { get; set; }
}