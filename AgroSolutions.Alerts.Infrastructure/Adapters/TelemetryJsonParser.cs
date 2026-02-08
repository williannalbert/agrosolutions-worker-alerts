using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using System.Text.Json.Nodes;

namespace AgroSolutions.Alerts.Infrastructure.Adapters;

public class TelemetryJsonParser : ITelemetryParser
{
    public TelemetryReading Parse(string rawPayload)
    {
        var node = JsonNode.Parse(rawPayload);
        if (node == null) throw new ArgumentException("JSON inválido");

        var dataNode = node["data"];
        if (dataNode == null) throw new ArgumentException("JSON inválido: propriedade 'data' ausente.");

        string deviceId = node["sensor_id"]?.ToString() ?? "Unknown";
        DateTime timestamp = node["time_stamp"]?.GetValue<DateTime>() ?? DateTime.UtcNow;

        string? typeSensor = node["type_sensor"]?.ToString();

        if (typeSensor == "solo" || dataNode["soil_moisture_percent"] != null)
        {
            return new TelemetryReading(
                DeviceId: deviceId,
                Timestamp: timestamp,
                SoilMoisture: dataNode["soil_moisture_percent"]?.GetValue<double>(),
                Temperature: null,
                Humidity: null,
                RainVolume: null
            );
        }

        if (typeSensor == "meteorologica" || dataNode["temp_celsius"] != null)
        {
            return new TelemetryReading(
                DeviceId: deviceId,
                Timestamp: timestamp,
                SoilMoisture: null,
                Temperature: dataNode["temp_celsius"]?.GetValue<double>(),
                Humidity: dataNode["humidity_percent"]?.GetValue<double>(),
                RainVolume: dataNode["rain_mm_last_hour"]?.GetValue<double>()
            );
        }

        throw new NotSupportedException($"Formato de sensor desconhecido ou tipo não suportado: {rawPayload}");
    }
}