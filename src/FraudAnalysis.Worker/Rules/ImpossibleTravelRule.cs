using FraudAnalysis.Application.Validators;
using FraudAnalysis.Domain.Entities;
using FraudAnalysis.Domain.Enums;
using FraudAnalysis.Domain.Interfaces;

namespace FraudAnalysis.Worker.Rules;

// Rejeita quando a velocidade implícita entre duas transações excede 1000 km/h.
public class ImpossibleTravelRule : IRiskRule
{
    private const double MaxSpeedKmPerHour = 1000;

    private readonly ITransactionRepository _repository;

    public ImpossibleTravelRule(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public RuleResult Evaluate(Transaction transaction)
    {
        if (GeoLocationValidator.IsNullIsland(transaction.Latitude, transaction.Longitude))
            return RuleResult.NotApplicable;

        var previous = _repository
            .GetLastByCustomerBeforeAsync(transaction.CustomerId, transaction.CreatedAt)
            .GetAwaiter()
            .GetResult();

        if (previous is null)
            return RuleResult.NotApplicable;

        if (GeoLocationValidator.IsNullIsland(previous.Latitude, previous.Longitude))
            return RuleResult.NotApplicable;

        var distanceKm = GeoLocationValidator.DistanceKm(
            previous.Latitude, previous.Longitude,
            transaction.Latitude, transaction.Longitude);

        var elapsedHours = (transaction.CreatedAt - previous.CreatedAt).TotalHours;

        if (elapsedHours <= 0)
            return RuleResult.Rejected;

        var impliedSpeedKmPerHour = distanceKm / elapsedHours;

        if (impliedSpeedKmPerHour > MaxSpeedKmPerHour)
            return RuleResult.Rejected;

        if (distanceKm > 500 && elapsedHours < 2)
            return RuleResult.Review;

        return RuleResult.Approved;
    }
}
