namespace FraudAnalysis.Domain.Enums;

/// <summary>
/// Resultado da análise de risco emitida pelo motor de decisão (RiskEngine).
/// </summary>
public enum FraudDecision
{
    /// <summary>Transação aprovada. Nenhuma regra de risco foi violada.</summary>
    Approved,

    /// <summary>Transação bloqueada. Uma ou mais regras críticas foram acionadas.</summary>
    Rejected,

    /// <summary>
    /// Transação sinalizada para revisão manual.
    /// Indica risco moderado que requer avaliação humana antes de prosseguir.
    /// </summary>
    Review
}
