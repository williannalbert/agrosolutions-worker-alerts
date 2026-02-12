using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class DroughtRiskSpecification : ISpecification<TelemetryReading>
{
    public const double DroughtThreshold = 30.0;
    public bool IsSatisfiedBy(TelemetryReading reading)
    {
        if (!reading.SoilMoisture.HasValue) return false;

        return reading.SoilMoisture.Value < DroughtThreshold;
    }
}