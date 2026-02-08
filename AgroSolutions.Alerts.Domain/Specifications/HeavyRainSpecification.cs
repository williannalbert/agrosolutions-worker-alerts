using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class HeavyRainSpecification : ISpecification<TelemetryReading>
{
    public bool IsSatisfiedBy(TelemetryReading reading)
    {
        if (!reading.RainVolume.HasValue) return false;

        return reading.RainVolume.Value > 50.0;
    }
}