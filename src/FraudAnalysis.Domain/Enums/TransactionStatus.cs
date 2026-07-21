namespace FraudAnalysis.Domain.Enums;

// Ciclo de vida de uma transação dentro do sistema.
public enum TransactionStatus
{
    Pending,
    Processing,
    Finished,
    Failed
}
