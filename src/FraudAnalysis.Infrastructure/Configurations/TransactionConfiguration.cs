using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FraudAnalysis.Infrastructure.Configurations;

/// <summary>
/// Mapeamento da entidade Transaction para a tabela "transactions" no PostgreSQL.
/// Separado do DbContext para manter o contexto limpo conforme o projeto cresce.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // o Id é gerado na camada de Application

        builder.Property(t => t.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(t => t.CustomerDocument)
            .HasColumnName("customer_document")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(t => t.Ip)
            .HasColumnName("ip")
            .HasMaxLength(45) // suporta IPv6
            .IsRequired();

        builder.Property(t => t.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(9, 6);

        builder.Property(t => t.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(9, 6);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()  // armazena "Pending", "Finished" etc. em vez de inteiro
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Decision)
            .HasColumnName("decision")
            .HasConversion<string?>()
            .HasMaxLength(20);

        builder.Property(t => t.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(128)
            .IsRequired();

        // Índice único: garante que duas transações não compartilhem a mesma chave.
        // Este índice é a base da implementação de idempotência (ADR-003).
        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("ix_transactions_idempotency_key");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.ProcessedAt)
            .HasColumnName("processed_at");
    }
}
