using AgroSolutions.Alerts.Domain.Enums;

namespace AgroSolutions.Alerts.Domain.Entities;

public class Alert
{
    public Guid Id { get; private set; }
    public string DeviceId { get; private set; }
    public string Message { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public string SuggestedFieldStatus { get; private set; }

    public Alert(string deviceId, string message, AlertSeverity severity, string suggestedFieldStatus)
    {
        Id = Guid.NewGuid();
        DeviceId = deviceId;
        Message = message;
        Severity = severity;
        SuggestedFieldStatus = suggestedFieldStatus;
        GeneratedAt = DateTime.UtcNow;
    }
}