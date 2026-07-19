using FraudAnalysis.Application.DTOs;

namespace FraudAnalysis.Application.Interfaces;

/// <summary>
/// Casos de uso expostos pela API para operações sobre transações.
/// A Controller conhece apenas esta interface — nunca a implementação concreta.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Recebe e persiste uma nova transação com status Pending,
    /// verificando idempotência antes de criar.
    /// Se a Idempotency-Key já existir, retorna a transação original sem criar outra.
    /// </summary>
    Task<TransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta o estado atual de uma transação pelo seu Id.
    /// Retorna null se não encontrada.
    /// </summary>
    Task<TransactionResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
