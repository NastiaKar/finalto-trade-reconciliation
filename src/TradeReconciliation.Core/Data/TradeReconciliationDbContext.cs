using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Domain;

namespace TradeReconciliation.Core.Data;

public class TradeReconciliationDbContext : DbContext
{
     public TradeReconciliationDbContext(DbContextOptions<TradeReconciliationDbContext> options) : base(options)
     {
     }

     public DbSet<Account> Accounts => Set<Account>();
     public DbSet<Trade> Trades => Set<Trade>();
     public DbSet<ReconciliationResult> ReconciliationResults => Set<ReconciliationResult>();

     protected override void OnModelCreating(ModelBuilder modelBuilder)
     {
          base.OnModelCreating(modelBuilder);

          modelBuilder.Entity<Trade>(t =>
          {
               t.Property(x => x.Price).HasColumnType("decimal(18,4)");
               t.Property(x => x.Quantity).HasColumnType("decimal(18,4)");

               t.HasIndex(x => new { x.TradeIdentifier, x.Source });
               t.HasIndex(x => x.TradeDate);
          });

          modelBuilder.Entity<Account>(a =>
          {
               a.HasIndex(x => x.AccountNumber).IsUnique();
          });

          modelBuilder.Entity<ReconciliationResult>(r =>
          {
               r.HasIndex(x => x.TradeIdentifier);
               r.HasIndex(x => x.ReconciledAt);
               r.HasIndex(x => x.TradeDate);
          });
     }
}