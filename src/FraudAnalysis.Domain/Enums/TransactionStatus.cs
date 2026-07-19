namespace FraudAnalysis.Domain.Enums;

/// <summary>
/// Representa o ciclo de vida de uma transação dentro do sistema.
/// </summary>
public enum TransactionStatus
{
    /// <summary>Transação recebida pela API, aguardando análise do Worker.</summary>
    Pending,

    /// <summary>Worker está processando ativamente a análise de risco.</summary>
    Processing,

    /// <summary>Análise concluída com sucesso. Verificar Decision para o resultado.</summary>
    Finished,

    /// <summary>Ocorreu um erro irrecuperável durante o processamento.</summary>
    Failed
}
