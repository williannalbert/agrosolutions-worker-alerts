using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class DroughtRiskSpecification : ISpecification<IEnumerable<TelemetryReading>>
{
    public bool IsSatisfiedBy(IEnumerable<TelemetryReading> history)
    {
        if (history == null || !history.Any()) return false;

        var lastReadings = history
            .Where(x => x.SoilMoisture.HasValue)
            .OrderByDescending(x => x.Timestamp)
            .Take(3)
            .ToList();

        if (lastReadings.Count < 3) return false;

        var averageMoisture = lastReadings.Average(x => x.SoilMoisture!.Value);

        return averageMoisture < 30.0;
    }
}