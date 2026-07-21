namespace FraudAnalysis.Domain.Enums;

// Veredito individual de uma regra de risco.
public enum RuleResult
{
    NotApplicable,
    Approved,
    Review,
    Rejected
}
