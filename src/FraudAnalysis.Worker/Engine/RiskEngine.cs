using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FraudAnalysis.Worker.Engine;

/// <summary>
/// Motor de decisão antifraude.
/// Executa todas as IRiskRule registradas e consolida o resultado.
///
/// Prioridade de decisão (da mais grave para a menos grave):
///   Rejected  >  Review  >  Approved
///
/// Se qualquer regra retornar Rejected, a transação é bloqueada imediatamente.
/// Se nenhuma regra retornar Rejected mas alguma retornar Review, vai para revisão.
/// Se nenhuma regra disparar, a transação é aprovada.
/// </summary>
public class RiskEngine
{
    private readonly IEnumerable<IRiskRule> _rules;
    private readonly ILogger<RiskEngine> _logger;

    public RiskEngine(IEnumerable<IRiskRule> rules, ILogger<RiskEngine> logger)
    {
        _rules  = rules;
        _logger = logger;
    }

    public FraudDecision Evaluate(Transaction transaction)
    {
        var finalDecision = FraudDecision.Approved;

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(transaction);

            if (result is null)
                continue;

            _logger.LogDebug(
                "Regra {Rule} retornou {Decision} para transação {TransactionId}",
                rule.GetType().Name, result, transaction.Id);

            // Rejected tem prioridade máxima — interrompe a avaliação
            if (result == FraudDecision.Rejected)
            {
                _logger.LogWarning(
                    "Transação {TransactionId} REJEITADA pela regra {Rule}",
                    transaction.Id, rule.GetType().Name);
                return FraudDecision.Rejected;
            }

            // Review tem prioridade sobre Approved mas não interrompe
            if (result == FraudDecision.Review)
                finalDecision = FraudDecision.Review;
        }

        _logger.LogInformation(
            "Transação {TransactionId} avaliada: {Decision}",
            transaction.Id, finalDecision);

        return finalDecision;
    }
}
