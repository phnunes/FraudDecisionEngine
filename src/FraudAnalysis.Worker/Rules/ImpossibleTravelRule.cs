using FraudAnalysis.Application.Validators;
using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

/// <summary>
/// Detecta "viagem impossível": quando o mesmo cliente realiza duas transações
/// em localizações geograficamente distantes em um intervalo de tempo inviável
/// para deslocamento físico.
///
/// Exemplo: transação em São Paulo e outra em Tóquio com 30 minutos de diferença.
///
/// Limiar: velocidade implícita maior que 1000 km/h (velocidade de avião comercial)
/// é considerada fisicamente impossível e indica conta comprometida ou fraude.
/// </summary>
public class ImpossibleTravelRule : IRiskRule
{
    private const double MaxSpeedKmPerHour = 1000;

    private readonly ITransactionRepository _repository;

    public ImpossibleTravelRule(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public FraudDecision? Evaluate(Transaction transaction)
    {
        // Coordenada (0,0) = sem dado de geolocalização — não avalia
        if (GeoLocationValidator.IsNullIsland(transaction.Latitude, transaction.Longitude))
            return null;

        var previous = _repository
            .GetLastByCustomerBeforeAsync(transaction.CustomerId, transaction.CreatedAt)
            .GetAwaiter()
            .GetResult();

        if (previous is null)
            return null;

        // Também sem geo na transação anterior — não avalia
        if (GeoLocationValidator.IsNullIsland(previous.Latitude, previous.Longitude))
            return null;

        var distanceKm = GeoLocationValidator.DistanceKm(
            previous.Latitude, previous.Longitude,
            transaction.Latitude, transaction.Longitude);

        var elapsedHours = (transaction.CreatedAt - previous.CreatedAt).TotalHours;

        // Evita divisão por zero para transações muito próximas no tempo
        if (elapsedHours < 0.001)
            elapsedHours = 0.001;

        var impliedSpeedKmPerHour = distanceKm / elapsedHours;

        if (impliedSpeedKmPerHour > MaxSpeedKmPerHour)
            return FraudDecision.Rejected;

        // Distância grande (> 500 km) em pouco tempo (< 2 horas) → revisão
        if (distanceKm > 500 && elapsedHours < 2)
            return FraudDecision.Review;

        return null;
    }
}
