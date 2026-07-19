using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

/// <summary>
/// Rejeita quando o mesmo cliente realiza mais de 5 transações em 1 minuto.
/// Indica automação maliciosa ou sequência de tentativas de fraude.
/// </summary>
public class FrequencyRule : IRiskRule
{
    private const int MaxTransactions = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private readonly ITransactionRepository _repository;

    public FrequencyRule(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public FraudDecision? Evaluate(Transaction transaction)
    {
        var count = _repository
            .CountRecentByCustomerAsync(transaction.CustomerId, Window)
            .GetAwaiter()
            .GetResult();

        if (count > MaxTransactions)
            return FraudDecision.Rejected;

        return null;
    }
}
