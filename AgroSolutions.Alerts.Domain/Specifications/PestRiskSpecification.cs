using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class PestRiskSpecification : ISpecification<WeatherReading>
{
    public bool IsSatisfiedBy(WeatherReading reading)
    {
        return reading.Temperature > 28.0 && reading.Humidity > 80.0;
    }
}