using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

/// <summary>
/// Rejeita transações com valor exatamente igual a R$ 100,00.
/// Padrão típico de testes de cartão clonado ("micro-teste" de validade).
/// </summary>
public class SuspiciousAmountRule : IRiskRule
{
    private const decimal SuspiciousAmount = 100m;

    public FraudDecision? Evaluate(Transaction transaction)
    {
        if (transaction.Amount == SuspiciousAmount)
            return FraudDecision.Rejected;

        return null;
    }
}
