namespace FraudAnalysis.Application.DTOs;

/// <summary>
/// Resposta retornada pela API ao criar ou consultar uma transação.
/// </summary>
public class TransactionResponse
{
    /// <summary>Identificador único da transação gerado pelo sistema.</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Status atual do ciclo de vida da transação.
    /// Valores possíveis: Pending, Processing, Finished, Failed.
    /// </summary>
    /// <example>Pending</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Decisão emitida pelo motor de risco após processamento.
    /// Nulo enquanto Status for Pending ou Processing.
    /// Valores possíveis: Approved, Rejected, Review.
    /// </summary>
    /// <example>null</example>
    public string? Decision { get; set; }

    /// <summary>Data e hora em que a transação foi recebida (UTC).</summary>
    /// <example>2026-07-13T20:00:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>Data e hora em que a análise foi concluída (UTC). Nulo se ainda não processada.</summary>
    /// <example>null</example>
    public DateTime? ProcessedAt { get; set; }
}
