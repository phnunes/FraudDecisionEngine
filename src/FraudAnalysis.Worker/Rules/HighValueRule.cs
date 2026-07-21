using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

// Sinaliza para revisão transações acima de R$ 10.000.
public class HighValueRule : IRiskRule
{
    private const decimal Threshold = 10_000m;

    public RuleResult Evaluate(Transaction transaction)
    {
        if (transaction.Amount > Threshold)
            return RuleResult.Review;

        return RuleResult.Approved;
    }
}
