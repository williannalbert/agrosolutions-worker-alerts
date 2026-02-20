using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Interfaces;

public interface ITelemetryRepository
{    
    Task SaveAlertAsync(Alert alert);
    Task<bool> ExistsRecentAlertAsync(string deviceId, string messageStart, TimeSpan period);
}                