using Prometheus;

namespace FraudAnalysis.Worker.Metrics;

/// <summary>
/// Métricas Prometheus expostas pelo Worker.
/// Scraping via /metrics no endpoint MetricServer (porta 9090).
/// </summary>
public static class WorkerMetrics
{
    /// <summary>Total de transações processadas com sucesso, rotuladas pela decisão.</summary>
    public static readonly Counter TransactionsProcessed = Prometheus.Metrics.CreateCounter(
        "fraud_transactions_processed_total",
        "Total de transações processadas pelo Worker.",
        new CounterConfiguration { LabelNames = ["decision"] });

    /// <summary>Total de falhas no processamento de mensagens.</summary>
    public static readonly Counter TransactionsFailed = Prometheus.Metrics.CreateCounter(
        "fraud_transactions_failed_total",
        "Total de falhas ao processar mensagens da fila.");

    /// <summary>Tempo de processamento de cada transação (ms).</summary>
    public static readonly Histogram ProcessingDuration = Prometheus.Metrics.CreateHistogram(
        "fraud_transaction_processing_duration_ms",
        "Duração do processamento de cada transação em milissegundos.",
        new HistogramConfiguration
        {
            Buckets = Histogram.LinearBuckets(start: 10, width: 50, count: 10)
        });

    /// <summary>Tamanho da fila de mensagens pendentes (gauge).</summary>
    public static readonly Gauge MessagesInFlight = Prometheus.Metrics.CreateGauge(
        "fraud_messages_in_flight",
        "Número de mensagens sendo processadas simultaneamente.");
}
