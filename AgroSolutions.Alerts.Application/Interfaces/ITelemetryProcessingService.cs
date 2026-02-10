namespace AgroSolutions.Alerts.Application.Interfaces;

public interface ITelemetryProcessingService
{
    Task ProcessMessageAsync(string rawJson);
}