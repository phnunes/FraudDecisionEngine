using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FraudAnalysis.Infrastructure.Context;

// Contexto EF Core principal — aplica configurações do assembly automaticamente.
public class FraudDbContext : DbContext
{
    public FraudDbContext(DbContextOptions<FraudDbContext> options)
        : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FraudDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
