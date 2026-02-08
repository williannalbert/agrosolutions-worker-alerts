using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.ValueObjects;

namespace AgroSolutions.Alerts.Domain.Interfaces;

public interface ITelemetryRepository
{    
    Task SaveAlertAsync(Alert alert);
}                