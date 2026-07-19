using System.Text;
using System.Text.Json;
using FraudAnalysis.Application.Events;
using FraudAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FraudAnalysis.Infrastructure.Messaging;

/// <summary>
/// Implementação de IMessagePublisher usando RabbitMQ.
///
/// Padrão de uso:
///   - Fila principal com DLQ configurada via argumentos x-dead-letter-exchange.
///   - Mensagens persistentes (DeliveryMode = Persistent) para sobreviver a restart do broker.
///   - Conexão criada por instância (Scoped) — adequado para publicação pontual na API.
/// </summary>
public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync<T>(
        string queue,
        T message,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);

        // Declara a DLQ primeiro (deve existir antes da fila principal)
        await _channel!.QueueDeclareAsync(
            queue:      QueueNames.FraudAnalysisDlq,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null,
            cancellationToken: cancellationToken);

        // Declara a fila principal com DLQ configurada
        var queueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", QueueNames.FraudAnalysisDlq }
        };

        await _channel.QueueDeclareAsync(
            queue:      queue,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  queueArgs,
            cancellationToken: cancellationToken);

        // Serializa o payload para JSON
        var json  = JsonSerializer.Serialize(message);
        var body  = Encoding.UTF8.GetBytes(json);

        // Propriedades da mensagem — persistente para sobreviver a restart do broker
        var properties = new BasicProperties
        {
            Persistent   = true,
            ContentType  = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(
            exchange:   string.Empty, // exchange padrão — roteia direto pelo nome da fila
            routingKey: queue,
            mandatory:  false,
            basicProperties: properties,
            body:       body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Mensagem publicada na fila {Queue}: {Payload}",
            queue, json);
    }

    // -------------------------------------------------------------------------
    // Conexão lazy — cria apenas quando a primeira mensagem for publicada
    // -------------------------------------------------------------------------
    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        var factory = new ConnectionFactory
        {
            HostName    = _settings.Host,
            Port        = _settings.Port,
            VirtualHost = _settings.VirtualHost,
            UserName    = _settings.Username,
            Password    = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel    = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Conexão com RabbitMQ estabelecida em {Host}:{Port}",
            _settings.Host, _settings.Port);
    }

    // -------------------------------------------------------------------------
    // Dispose — libera canal e conexão ao fim do escopo
    // -------------------------------------------------------------------------
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}
