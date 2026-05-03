using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Services;

public class CsvTradeImporter : ITradeImporter
{
    private readonly TradeReconciliationDbContext _db;

    private static readonly string[] ExpectedHeaders = new[]
    {
        "TradeIdentifier", "Symbol", "Side", "Quantity", "Price", "TradeDate", "Counterparty", "AccountNumber"
    };

    public CsvTradeImporter(TradeReconciliationDbContext db)
    {
        _db = db;
    }
    
    public async Task<TradeImportResult> ImportCsvAsync(
        Stream csvStream, 
        TradeSource source, 
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var importedTrades = new List<Trade>();

        var accountsByNumber = await _db.Accounts
            .ToDictionaryAsync(a => a.AccountNumber, cancellationToken);
        
        using var reader = new StreamReader(csvStream);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine is null)
        {
            errors.Add("File is empty.");
            return new TradeImportResult(0, 0, 0, errors);
        }
        
        var headers = headerLine.Split(',').Select(h => h.Trim()).ToArray();
        if (!headers.SequenceEqual(ExpectedHeaders))
        {
            errors.Add($"Unexpected header. Expected: {string.Join(",",  ExpectedHeaders)}. " +
                       $"Got: {string.Join(",", headers)}");
            return new TradeImportResult(0, 0, 0, errors);
        }

        var lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = line.Split(',');
            if (fields.Length != ExpectedHeaders.Length)
            {
                errors.Add($"Line {lineNumber}: expected: {ExpectedHeaders.Length} columns, " +
                           $"got {fields.Length}.");
                continue;
            }

            try
            {
                var trade = ParseRow(fields, source, accountsByNumber, lineNumber);
                importedTrades.Add(trade);
            }
            catch (Exception ex)
            {
                errors.Add($"Line:  {lineNumber}: {ex.Message}.");
            }
        }

        if (importedTrades.Count > 0)
        {
            _db.Trades.AddRange(importedTrades);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new TradeImportResult(
            TotalRows: lineNumber - 1,
            ImportedCount: importedTrades.Count,
            RejectedCount: errors.Count,
            Errors: errors);
    }

    private static Trade ParseRow(
        string[] fields,
        TradeSource source,
        Dictionary<string, Account> accountsByNumber,
        int lineNumber)
    {
        var tradeIdentifier = fields[0].Trim();
        var symbol = fields[1].Trim();
        var sideStr = fields[2].Trim();
        var quantityStr = fields[3].Trim();
        var priceStr = fields[4].Trim();
        var tradeDateStr = fields[5].Trim();
        var counterparty = fields[6].Trim();
        var accountNumber = fields[7].Trim();

        if (!Enum.TryParse<TradeSide>(sideStr, ignoreCase: true, out var side))
            throw new FormatException($"Invalid side '{sideStr}', expected Buy or Sell.");

        if (!decimal.TryParse(quantityStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity))
            throw new FormatException($"Invalid quantity '{quantityStr}'.");

        if (!decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            throw new FormatException($"Invalid price '{priceStr}'.");

        if (!DateTime.TryParse(tradeDateStr, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var tradeDate))
            throw new FormatException($"Invalid TradeDate '{tradeDateStr}'.");

        if (!accountsByNumber.TryGetValue(accountNumber, out var account))
            throw new InvalidOperationException($"Unknown AccountNumber '{accountNumber}'.");

        return new Trade
        {
            TradeIdentifier = tradeIdentifier,
            Source = source,
            Symbol = symbol,
            Side = side,
            Quantity = quantity,
            Price = price,
            TradeDate = tradeDate,
            Counterparty = counterparty,
            AccountId = account.Id,
        };
    }
}