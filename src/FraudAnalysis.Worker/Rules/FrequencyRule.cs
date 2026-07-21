using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

// Rejeita quando o cliente realiza mais de 5 transações em 1 minuto.
public class FrequencyRule : IRiskRule
{
    private const int MaxTransactions = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private readonly ITransactionRepository _repository;

    public FrequencyRule(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public RuleResult Evaluate(Transaction transaction)
    {
        var count = _repository
            .CountRecentByCustomerAsync(transaction.CustomerId, Window)
            .GetAwaiter()
            .GetResult();

        if (count > MaxTransactions)
            return RuleResult.Rejected;

        return RuleResult.Approved;
    }
}
