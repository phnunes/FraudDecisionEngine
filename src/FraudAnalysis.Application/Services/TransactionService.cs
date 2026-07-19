using FraudAnalysis.Application.DTOs;
using FraudAnalysis.Application.Events;
using FraudAnalysis.Application.Interfaces;
using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Application.Services;

/// <summary>
/// Implementa os casos de uso de criação e consulta de transações.
/// Responsável por: validar idempotência, mapear DTOs, orquestrar persistência
/// e publicar o evento de criação na fila RabbitMQ.
/// Não conhece EF Core, SQL ou detalhes de infraestrutura.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly IMessagePublisher _publisher;

    public TransactionService(
        ITransactionRepository repository,
        IMessagePublisher publisher)
    {
        _repository = repository;
        _publisher  = publisher;
    }

    /// <inheritdoc />
    public async Task<TransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        // Idempotência: se já existe uma transação com essa chave, retorna ela
        var existing = await _repository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
            return MapToResponse(existing);

        var transaction = new Transaction
        {
            Id               = Guid.NewGuid(),
            CustomerId       = request.CustomerId,
            CustomerDocument = request.CustomerDocument.Trim(),
            Amount           = request.Amount,
            Currency         = request.Currency.ToUpperInvariant(),
            Ip               = request.Ip.Trim(),
            Latitude         = request.Latitude,
            Longitude        = request.Longitude,
            IdempotencyKey   = idempotencyKey,
            CreatedAt        = DateTime.UtcNow
        };

        await _repository.AddAsync(transaction, cancellationToken);

        // Publica o evento na fila — o Worker consumirá e aplicará as regras de risco
        await _publisher.PublishAsync(
            QueueNames.FraudAnalysis,
            new TransactionCreatedEvent(transaction.Id),
            cancellationToken);

        return MapToResponse(transaction);
    }

    /// <inheritdoc />
    public async Task<TransactionResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(id, cancellationToken);
        return transaction is null ? null : MapToResponse(transaction);
    }

    // -------------------------------------------------------------------------
    // Mapeamento interno — mantido simples e explícito (sem AutoMapper)
    // -------------------------------------------------------------------------
    private static TransactionResponse MapToResponse(Transaction t) => new()
    {
        Id          = t.Id,
        Status      = t.Status.ToString(),
        Decision    = t.Decision?.ToString(),
        CreatedAt   = t.CreatedAt,
        ProcessedAt = t.ProcessedAt
    };
}
