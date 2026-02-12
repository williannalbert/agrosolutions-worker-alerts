namespace AgroSolutions.Alerts.Domain.ValueObjects;

public abstract record TelemetryReading(
    string DeviceId,
    DateTime Timestamp
);

public record SoilReading(
    string DeviceId,
    DateTime Timestamp,
    double SoilMoisture, 
    double SoilPh,
    SoilNutrients Nutrients 
) : TelemetryReading(DeviceId, Timestamp);

public record SoilNutrients(double Nitrogen, double Phosphorus, double Potassium);

public record WeatherReading(
    string DeviceId,
    DateTime Timestamp,
    double Temperature,
    double Humidity,
    double RainVolume,
    double WindSpeed
) : TelemetryReading(DeviceId, Timestamp);

public record SiloReading(
    string DeviceId,
    DateTime Timestamp,
    double FillLevel,
    double Co2Level,
    double InternalTemp
) : TelemetryReading(DeviceId, Timestamp);