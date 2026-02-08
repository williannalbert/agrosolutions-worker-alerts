namespace AgroSolutions.Alerts.Domain.ValueObjects;

public record TelemetryReading(
    string DeviceId,
    DateTime Timestamp,
    double? SoilMoisture,
    double? Temperature,
    double? Humidity,
    double? RainVolume
);
