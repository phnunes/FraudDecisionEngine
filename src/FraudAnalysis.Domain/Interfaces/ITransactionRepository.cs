using FraudAnalysis.Domain.Entities;

namespace FraudAnalysis.Domain.Interfaces;

// Contrato de acesso a dados para transações.
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<int> CountRecentByCustomerAsync(Guid customerId, TimeSpan window, CancellationToken cancellationToken = default);
    Task<Transaction?> GetLastByCustomerBeforeAsync(Guid customerId, DateTime before, CancellationToken cancellationToken = default);
}
