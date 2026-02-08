using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class PestRiskSpecification : ISpecification<TelemetryReading>
{
    public bool IsSatisfiedBy(TelemetryReading reading)
    {
        if (!reading.Temperature.HasValue || !reading.Humidity.HasValue) return false;

        return reading.Temperature.Value > 28.0 && reading.Humidity.Value > 80.0;
    }
}