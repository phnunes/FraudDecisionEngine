namespace FraudAnalysis.Application.DTOs;

// Resposta retornada pela API ao criar ou consultar uma transação.
public class TransactionResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Decision { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
