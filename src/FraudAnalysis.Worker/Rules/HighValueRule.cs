using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

/// <summary>
/// Sinaliza para revisão manual transações com valor acima de R$ 10.000.
/// Valores altos não são necessariamente fraude, mas exigem avaliação adicional.
/// </summary>
public class HighValueRule : IRiskRule
{
    private const decimal Threshold = 10_000m;

    public FraudDecision? Evaluate(Transaction transaction)
    {
        if (transaction.Amount > Threshold)
            return FraudDecision.Review;

        return null;
    }
}
