using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Services;

public interface ITradeImporter
{
    /// <summary>
    /// Parses a CSV stream of trade rows and saves them with the given source.
    /// Bad rows are reported as errors but don't abort the import.
    /// </summary>
    Task<TradeImportResult> ImportCsvAsync(
        Stream csvStream,
        TradeSource source,
        CancellationToken cancellationToken = default);
}