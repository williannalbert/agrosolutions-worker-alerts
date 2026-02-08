using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Application.Interfaces;

public interface ITelemetryParser
{
    TelemetryReading Parse(string rawPayload);
}