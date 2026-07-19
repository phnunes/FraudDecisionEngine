using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FraudAnalysis.Infrastructure.Context;

/// <summary>
/// Contexto principal do EF Core.
/// Mantém apenas o DbSet necessário; toda configuração de mapeamento
/// é delegada para as classes IEntityTypeConfiguration (pasta Configurations).
/// </summary>
public class FraudDbContext : DbContext
{
    public FraudDbContext(DbContextOptions<FraudDbContext> options)
        : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações encontradas no assembly da Infrastructure
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FraudDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
