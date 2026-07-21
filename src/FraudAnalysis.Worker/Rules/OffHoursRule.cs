using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

// Sinaliza para revisão transações realizadas entre 00h e 05h (Brasília).
public class OffHoursRule : IRiskRule
{
    private static readonly TimeZoneInfo BrasiliaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private const int SuspiciousStartHour = 0;
    private const int SuspiciousEndHour   = 5;

    public RuleResult Evaluate(Transaction transaction)
    {
        DateTime localTime;

        try
        {
            localTime = TimeZoneInfo.ConvertTimeFromUtc(transaction.CreatedAt, BrasiliaTimeZone);
        }
        catch
        {
            localTime = transaction.CreatedAt;
        }

        var hour = localTime.Hour;

        if (hour >= SuspiciousStartHour && hour < SuspiciousEndHour)
            return RuleResult.Review;

        return RuleResult.Approved;
    }
}
