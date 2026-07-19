using FraudAnalysis.Domain.Entities;

namespace FraudAnalysis.Domain.Interfaces;

/// <summary>
/// Contrato de acesso a dados para transações.
/// O Domain define o que precisa; a Infrastructure implementa como.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Busca uma transação pelo seu identificador único.</summary>
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca uma transação pela chave de idempotência.
    /// Retorna null se nenhuma transação com essa chave existir.
    /// </summary>
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>Persiste uma nova transação no banco de dados.</summary>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>Atualiza os dados de uma transação existente (status, decisão, processedAt).</summary>
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta quantas transações o cliente realizou dentro da janela de tempo informada.
    /// Usado pela FrequencyRule para detectar rajadas suspeitas de transações.
    /// </summary>
    Task<int> CountRecentByCustomerAsync(Guid customerId, TimeSpan window, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna a transação mais recente do cliente anterior ao momento informado.
    /// Usado pela ImpossibleTravelRule para calcular velocidade de deslocamento.
    /// </summary>
    Task<Transaction?> GetLastByCustomerBeforeAsync(Guid customerId, DateTime before, CancellationToken cancellationToken = default);
}
