using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Specifications;

public class HeavyRainSpecification : ISpecification<WeatherReading>
{
    public bool IsSatisfiedBy(WeatherReading reading)
    {
        return reading.RainVolume > 50.0;
    }
}