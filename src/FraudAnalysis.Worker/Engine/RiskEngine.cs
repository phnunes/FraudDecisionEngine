using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Engine;

// Motor de decisão antifraude — executa todas as IRiskRule e consolida o veredito.
public class RiskEngine
{
    private readonly IEnumerable<IRiskRule> _rules;

    public RiskEngine(IEnumerable<IRiskRule> rules)
    {
        _rules = rules;
    }

    public FraudDecision Evaluate(Transaction transaction)
    {
        var finalDecision = FraudDecision.Approved;

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(transaction);

            if (result == RuleResult.NotApplicable)
                continue;

            if (result == RuleResult.Rejected)
                return FraudDecision.Rejected;

            if (result == RuleResult.Review)
                finalDecision = FraudDecision.Review;
        }

        return finalDecision;
    }
}
