using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

/// <summary>
/// Sinaliza para revisão transações realizadas em horários atípicos.
/// Transações entre 00:00 e 05:00 (UTC-3 / Brasília) têm risco elevado
/// pois a maioria dos clientes legítimos não opera nesse período.
///
/// Não rejeita automaticamente — apenas sinaliza para revisão humana.
/// </summary>
public class OffHoursRule : IRiskRule
{
    // Horário de Brasília = UTC-3
    private static readonly TimeZoneInfo BrasiliaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private const int SuspiciousStartHour = 0;  // meia-noite
    private const int SuspiciousEndHour   = 5;  // 05:00

    public FraudDecision? Evaluate(Transaction transaction)
    {
        DateTime localTime;

        try
        {
            localTime = TimeZoneInfo.ConvertTimeFromUtc(transaction.CreatedAt, BrasiliaTimeZone);
        }
        catch
        {
            // Se não conseguir converter, usa UTC mesmo
            localTime = transaction.CreatedAt;
        }

        var hour = localTime.Hour;

        if (hour >= SuspiciousStartHour && hour < SuspiciousEndHour)
            return FraudDecision.Review;

        return null;
    }
}
