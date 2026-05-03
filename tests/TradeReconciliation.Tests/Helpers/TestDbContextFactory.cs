using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Data;

namespace TradeReconciliation.Tests.Helpers;

public static class TestDbContextFactory
{
    public static TradeReconciliationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TradeReconciliationDbContext>().
            UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new TradeReconciliationDbContext(options);
    }
}