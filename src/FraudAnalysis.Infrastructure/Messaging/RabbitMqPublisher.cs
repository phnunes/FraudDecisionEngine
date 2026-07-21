using System.Text;
using System.Text.Json;
using FraudAnalysis.Application.Events;
using FraudAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FraudAnalysis.Infrastructure.Messaging;

// Publica mensagens no RabbitMQ com mensagens persistentes e DLQ configurada.
public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task PublishAsync<T>(
        string queue,
        T message,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);

        await _channel!.QueueDeclareAsync(
            queue:      QueueNames.FraudAnalysisDlq,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null,
            cancellationToken: cancellationToken);

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

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent   = true,
            ContentType  = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(
            exchange:        string.Empty,
            routingKey:      queue,
            mandatory:       false,
            basicProperties: properties,
            body:            body,
            cancellationToken: cancellationToken);
    }

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
    }

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
