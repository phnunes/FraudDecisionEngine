using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;

namespace FraudAnalysis.Domain.Interfaces;

// Contrato para cada regra de risco aplicada pelo RiskEngine.
public interface IRiskRule
{
    RuleResult Evaluate(Transaction transaction);
}
