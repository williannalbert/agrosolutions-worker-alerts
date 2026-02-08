using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Application.Interfaces;

public interface IHistoryIntegrationService
{
    Task RegisterReadingAsync(TelemetryReading reading);
    Task<bool> HasHealthyMoistureInPeriodAsync(string deviceId, int hours);
}
