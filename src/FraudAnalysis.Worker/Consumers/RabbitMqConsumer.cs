using System.Text;
using System.Text.Json;
using FraudAnalysis.Application.Events;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure.Messaging;
using FraudAnalysis.Worker.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FraudAnalysis.Worker.Consumers;

// Consome mensagens da fila RabbitMQ e aciona o RiskEngine para cada transação.
public class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;

    private IConnection? _connection;
    private IChannel? _channel;

    private const int MaxRetries = 3;

    public RabbitMqConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _settings     = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
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

        await _channel.QueueDeclareAsync(
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
            queue:      QueueNames.FraudAnalysis,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  queueArgs,
            cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue:       QueueNames.FraudAnalysis,
            autoAck:     false,
            consumer:    consumer,
            cancellationToken: cancellationToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body    = ea.Body.ToArray();
        var json    = Encoding.UTF8.GetString(body);
        var retries = GetRetryCount(ea);

        try
        {
            var @event = JsonSerializer.Deserialize<TransactionCreatedEvent>(json)
                ?? throw new InvalidOperationException($"Payload inválido: {json}");

            await ProcessTransactionAsync(@event.TransactionId);

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch
        {
            var requeue = retries < MaxRetries;
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: requeue);
        }
    }

    private async Task ProcessTransactionAsync(Guid transactionId)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var riskEngine = scope.ServiceProvider.GetRequiredService<RiskEngine>();

        var transaction = await repository.GetByIdAsync(transactionId);

        if (transaction is null)
            return;

        transaction.Status = TransactionStatus.Processing;
        await repository.UpdateAsync(transaction);

        var decision = riskEngine.Evaluate(transaction);

        transaction.Decision    = decision;
        transaction.Status      = TransactionStatus.Finished;
        transaction.ProcessedAt = DateTime.UtcNow;
        await repository.UpdateAsync(transaction);
    }

    private static int GetRetryCount(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties.Headers is not null &&
            ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var value) &&
            value is int count)
        {
            return count;
        }

        return 0;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection.Dispose();
        }
    }
}
