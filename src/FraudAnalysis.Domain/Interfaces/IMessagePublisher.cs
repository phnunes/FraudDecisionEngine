namespace FraudAnalysis.Domain.Interfaces;

// Contrato para publicação de mensagens em broker de mensageria.
public interface IMessagePublisher
{
    Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default);
}
