using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Application.Interfaces;

public interface IHistoryIntegrationService
{
    Task RegisterReadingAsync(TelemetryReading reading);
    Task<IEnumerable<TelemetryReading>> GetHistoryAsync(string deviceId, TimeSpan period);
}
