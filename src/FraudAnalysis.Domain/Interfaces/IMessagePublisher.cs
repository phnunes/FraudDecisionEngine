namespace FraudAnalysis.Domain.Interfaces;

/// <summary>
/// Contrato para publicação de mensagens em um broker de mensageria.
/// O Domain define o que precisa; a Infrastructure implementa com RabbitMQ.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publica uma mensagem na fila especificada.
    /// </summary>
    /// <typeparam name="T">Tipo do payload — será serializado para JSON.</typeparam>
    /// <param name="queue">Nome da fila de destino.</param>
    /// <param name="message">Payload da mensagem.</param>
    /// <param name="cancellationToken"></param>
    Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default);
}
