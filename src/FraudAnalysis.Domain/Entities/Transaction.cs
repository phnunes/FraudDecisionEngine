using FraudAnalysis.Domain.Enums;

namespace FraudAnalysis.Domain.Entities;

/// <summary>
/// Representa uma transação financeira submetida para análise de risco.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }

    /// <summary>Identificador do cliente que originou a transação.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>CPF do cliente (somente dígitos, 11 caracteres).</summary>
    public string CustomerDocument { get; set; } = string.Empty;

    /// <summary>Valor da transação.</summary>
    public decimal Amount { get; set; }

    /// <summary>Moeda da transação (ex: BRL, USD).</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Endereço IP de origem da requisição.</summary>
    public string Ip { get; set; } = string.Empty;

    /// <summary>Latitude geográfica do dispositivo no momento da transação.</summary>
    public decimal Latitude { get; set; }

    /// <summary>Longitude geográfica do dispositivo no momento da transação.</summary>
    public decimal Longitude { get; set; }

    /// <summary>Status atual do processamento da transação.</summary>
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    /// <summary>
    /// Decisão emitida pelo motor de risco após análise.
    /// Nulo enquanto a transação ainda não foi processada.
    /// </summary>
    public FraudDecision? Decision { get; set; }

    /// <summary>
    /// Chave de idempotência fornecida pelo cliente.
    /// Garante que reenvios da mesma requisição não criem transações duplicadas.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>Momento em que a transação foi recebida pela API (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Momento em que o Worker concluiu a análise (UTC).</summary>
    public DateTime? ProcessedAt { get; set; }
}
