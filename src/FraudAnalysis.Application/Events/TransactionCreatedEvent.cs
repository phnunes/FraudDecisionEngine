namespace FraudAnalysis.Application.Events;

/// <summary>
/// Mensagem publicada na fila RabbitMQ após uma transação ser salva com sucesso.
/// Intencionalmente minimalista — o Worker busca os dados completos no banco
/// usando o TransactionId, evitando payloads grandes na fila.
/// </summary>
public record TransactionCreatedEvent(Guid TransactionId);
