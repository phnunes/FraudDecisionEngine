namespace FraudAnalysis.Application.Events;

// Nomes das filas RabbitMQ usadas pelo sistema.
public static class QueueNames
{
    public const string FraudAnalysis = "fraud-analysis";
    public const string FraudAnalysisDlq = "fraud-analysis-dlq";
}
