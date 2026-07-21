namespace FraudAnalysis.Application.Events;

// Evento publicado na fila após uma transação ser persistida.
public record TransactionCreatedEvent(Guid TransactionId);
