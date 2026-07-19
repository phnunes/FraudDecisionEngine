using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;

namespace FraudAnalysis.Domain.Interfaces;

/// <summary>
/// Contrato para cada regra de risco individual aplicada pelo RiskEngine.
/// Implementar esta interface para adicionar novas regras sem alterar o motor.
/// </summary>
public interface IRiskRule
{
    /// <summary>
    /// Avalia a transação e retorna a decisão desta regra específica,
    /// ou null se a regra não se aplica a esta transação.
    /// </summary>
    FraudDecision? Evaluate(Transaction transaction);
}
