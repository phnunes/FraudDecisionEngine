using FraudAnalysis.Application.Validators;
using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

/// <summary>
/// Sinaliza para revisão transações originadas de IPs privados/internos.
/// IPs privados (10.x, 192.168.x, 172.16-31.x) não deveriam originar
/// transações financeiras externas — indica possível proxy interno ou teste.
/// </summary>
public class PrivateIpRule : IRiskRule
{
    public FraudDecision? Evaluate(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Ip))
            return FraudDecision.Review;

        if (!IpValidator.IsValid(transaction.Ip))
            return FraudDecision.Rejected;

        if (IpValidator.IsPrivate(transaction.Ip))
            return FraudDecision.Review;

        return null;
    }
}
