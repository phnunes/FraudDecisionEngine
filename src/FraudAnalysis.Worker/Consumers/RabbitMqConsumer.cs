using System.Text;
using System.Text.Json;
using FraudAnalysis.Application.Events;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure.Messaging;
using FraudAnalysis.Worker.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FraudAnalysis.Worker.Consumers;

/// <summary>
/// BackgroundService que escuta a fila RabbitMQ e aciona o RiskEngine
/// para cada transação recebida.
///
/// Fluxo por mensagem:
///   1. Deserializa TransactionCreatedEvent
///   2. Busca a Transaction completa no banco
///   3. Atualiza status para Processing
///   4. Executa RiskEngine → obtém Decision
///   5. Atualiza status para Finished + grava Decision
///   6. ACK na mensagem
///
/// Em caso de erro:
///   - Até 3 tentativas com requeue
///   - Na 4ª falha: NACK sem requeue → mensagem vai para a DLQ
/// </summary>
public class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    private const int MaxRetries = 3;

    public RabbitMqConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _settings     = settings.Value;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAsync(stoppingToken);

        _logger.LogInformation(
            "Worker aguardando mensagens na fila '{Queue}'...",
            QueueNames.FraudAnalysis);

        // Mantém o host vivo enquanto não for cancelado
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    // -------------------------------------------------------------------------
    // Conexão e configuração do consumer
    // -------------------------------------------------------------------------
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

        // Declara DLQ
        await _channel.QueueDeclareAsync(
            queue:      QueueNames.FraudAnalysisDlq,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null,
            cancellationToken: cancellationToken);

        // Declara fila principal com DLQ configurada
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

        // Processa uma mensagem por vez — garante ordenação e controla carga
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue:       QueueNames.FraudAnalysis,
            autoAck:     false, // ACK manual — só confirma após processar com sucesso
            consumer:    consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Conectado ao RabbitMQ em {Host}:{Port}",
            _settings.Host, _settings.Port);
    }

    // -------------------------------------------------------------------------
    // Handler de cada mensagem recebida
    // -------------------------------------------------------------------------
    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body    = ea.Body.ToArray();
        var json    = Encoding.UTF8.GetString(body);
        var retries = GetRetryCount(ea);

        _logger.LogInformation(
            "Mensagem recebida (tentativa {Attempt}/{Max}): {Payload}",
            retries + 1, MaxRetries, json);

        try
        {
            var @event = JsonSerializer.Deserialize<TransactionCreatedEvent>(json);

            if (@event is null)
                throw new InvalidOperationException($"Payload inválido: {json}");

            await ProcessTransactionAsync(@event.TransactionId);

            // Sucesso — confirma a mensagem
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao processar mensagem (tentativa {Attempt}/{Max})",
                retries + 1, MaxRetries);

            var requeue = retries < MaxRetries;

            // NACK: se ainda tem tentativas, recoloca na fila; senão vai para DLQ
            await _channel!.BasicNackAsync(
                deliveryTag: ea.DeliveryTag,
                multiple:    false,
                requeue:     requeue);

            if (!requeue)
                _logger.LogWarning(
                    "Mensagem enviada para DLQ após {MaxRetries} tentativas: {Payload}",
                    MaxRetries, json);
        }
    }

    // -------------------------------------------------------------------------
    // Processamento da transação
    // -------------------------------------------------------------------------
    private async Task ProcessTransactionAsync(Guid transactionId)
    {
        // Cria um escopo por mensagem — garante DbContext e repositório isolados
        await using var scope = _scopeFactory.CreateAsyncScope();

        var repository  = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var riskEngine  = scope.ServiceProvider.GetRequiredService<RiskEngine>();

        var transaction = await repository.GetByIdAsync(transactionId);

        if (transaction is null)
        {
            _logger.LogWarning(
                "Transação {TransactionId} não encontrada no banco. Mensagem descartada.",
                transactionId);
            return;
        }

        // Marca como em processamento
        transaction.Status = TransactionStatus.Processing;
        await repository.UpdateAsync(transaction);

        // Executa o motor de decisão
        var decision = riskEngine.Evaluate(transaction);

        // Persiste o resultado
        transaction.Decision    = decision;
        transaction.Status      = TransactionStatus.Finished;
        transaction.ProcessedAt = DateTime.UtcNow;
        await repository.UpdateAsync(transaction);

        _logger.LogInformation(
            "Transação {TransactionId} finalizada — Decision: {Decision}",
            transactionId, decision);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
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
