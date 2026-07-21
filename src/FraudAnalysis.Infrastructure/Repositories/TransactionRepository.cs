using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FraudAnalysis.Infrastructure.Repositories;

// Implementação de ITransactionRepository com EF Core + PostgreSQL.
public class TransactionRepository : ITransactionRepository
{
    private readonly FraudDbContext _context;

    public TransactionRepository(FraudDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Transaction?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountRecentByCustomerAsync(
        Guid customerId,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow - window;
        return await _context.Transactions
            .CountAsync(t => t.CustomerId == customerId && t.CreatedAt >= since, cancellationToken);
    }

    public async Task<Transaction?> GetLastByCustomerBeforeAsync(
        Guid customerId,
        DateTime before,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId && t.CreatedAt < before)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
