namespace TradeReconciliation.Core.Services;

public record TradeImportResult(
    int TotalRows,
    int ImportedCount,
    int RejectedCount,
    List<string> Errors);