using FraudAnalysis.Application.DTOs;

namespace FraudAnalysis.Application.Interfaces;

// Casos de uso para operações sobre transações.
public interface ITransactionService
{
    Task<TransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<TransactionResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
