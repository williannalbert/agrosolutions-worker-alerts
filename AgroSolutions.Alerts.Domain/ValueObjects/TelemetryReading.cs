namespace AgroSolutions.Alerts.Domain.ValueObjects;

public abstract record TelemetryReading(
    string DeviceId,
    DateTime Timestamp,
    Guid FieldId
);

public record SoilReading(
    string DeviceId,
    DateTime Timestamp,
    Guid FieldId,
    double SoilMoisture, 
    double SoilPh,
    SoilNutrients Nutrients 
) : TelemetryReading(DeviceId, Timestamp, FieldId);

public record SoilNutrients(double Nitrogen, double Phosphorus, double Potassium);

public record WeatherReading(
    string DeviceId,
    DateTime Timestamp,
    Guid FieldId,
    double Temperature,
    double Humidity,
    double RainVolume,
    double WindSpeed,
    string WindDirection,
    double DewPoint
) : TelemetryReading(DeviceId, Timestamp, FieldId);

public record SiloReading(
    string DeviceId,
    DateTime Timestamp,
    Guid FieldId,
    double FillLevel,
    double Co2Level,
    double InternalTemp
) : TelemetryReading(DeviceId, Timestamp, FieldId);