using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class DroughtRiskSpecification : ISpecification<SoilReading>
{
    public const double DroughtThreshold = 30.0;
    public bool IsSatisfiedBy(SoilReading reading)
    {
        return reading.SoilMoisture < DroughtThreshold;
    }
}