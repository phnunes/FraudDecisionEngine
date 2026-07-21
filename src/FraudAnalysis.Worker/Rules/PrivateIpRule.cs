using FraudAnalysis.Application.Validators;
using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

// Sinaliza para revisão transações originadas de IPs privados ou inválidos.
public class PrivateIpRule : IRiskRule
{
    public RuleResult Evaluate(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Ip))
            return RuleResult.Review;

        if (!IpValidator.IsValid(transaction.Ip))
            return RuleResult.Rejected;

        if (IpValidator.IsPrivate(transaction.Ip))
            return RuleResult.Review;

        return RuleResult.Approved;
    }
}
