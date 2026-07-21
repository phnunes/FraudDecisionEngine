using FraudAnalysis.Domain.Enums;

namespace FraudAnalysis.Domain.Entities;

// Transação financeira submetida para análise de risco.
public class Transaction
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerDocument { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public FraudDecision? Decision { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
