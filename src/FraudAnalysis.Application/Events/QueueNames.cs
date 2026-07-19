namespace FraudAnalysis.Application.Events;

/// <summary>
/// Nomes das filas RabbitMQ usadas pelo sistema.
/// Centralizado aqui para evitar strings literais espalhadas pelo código.
/// </summary>
public static class QueueNames
{
    /// <summary>Fila principal de análise de fraude.</summary>
    public const string FraudAnalysis = "fraud-analysis";

    /// <summary>Dead Letter Queue — mensagens que falharam após todas as tentativas.</summary>
    public const string FraudAnalysisDlq = "fraud-analysis-dlq";
}
