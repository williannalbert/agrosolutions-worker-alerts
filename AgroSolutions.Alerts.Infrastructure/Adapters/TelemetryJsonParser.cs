using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgroSolutions.Alerts.Infrastructure.Adapters;

public class TelemetryJsonParser : ITelemetryParser
{
    public TelemetryReading Parse(string rawPayload)
    {
        var node = JsonNode.Parse(rawPayload);
        if (node == null) throw new ArgumentException("JSON inválido");

        var fieldId = node["fieldId"]?.GetValue<Guid>() ?? Guid.Empty;
        string deviceId = node["sensorId"]?.ToString() ?? "Unknown";
        DateTime timestamp = node["timeStamp"]?.GetValue<DateTime>() ?? DateTime.UtcNow;

        var typeSensorRaw = node["typeSensor"]?.ToString();
        var data = node["data"];

        if (data == null) throw new ArgumentException("JSON inválido: propriedade 'data' ausente.");

        string typeSensor = MapSensorType(typeSensorRaw);

        return typeSensor switch
        {
            "solo" => ParseSoil(deviceId, timestamp, fieldId, data),
            "meteorologica" => ParseWeather(deviceId, timestamp, fieldId, data),
            "silo" => ParseSilo(deviceId, timestamp, fieldId, data),
            _ => throw new NotSupportedException($"Tipo de sensor desconhecido ou não suportado: {typeSensorRaw}")
        };
    }

    private string MapSensorType(string? rawType)
    {
        if (string.IsNullOrWhiteSpace(rawType)) return "unknown";

        var normalized = rawType.ToLower().Trim();

        if (int.TryParse(rawType, out int typeId))
        {
            return typeId switch
            {
                0 => "solo",
                1 => "meteorologica",
                2 => "silo",
                _ => "unknown"
            };
        }

        if (normalized.Contains("solo")) return "solo";
        if (normalized.Contains("meteorologica") || normalized.Contains("clima")) return "meteorologica";
        if (normalized.Contains("silo")) return "silo";

        return normalized;
    }

    private SoilReading ParseSoil(string id, DateTime time, Guid fieldId, JsonNode data)
    {
        var nutrientesNode = data["nutrientesData"]; 

        return new SoilReading(
            DeviceId: id,
            Timestamp: time,
            FieldId: fieldId,
            SoilMoisture: data["umidade"]?.GetValue<double>() ?? 0,
            SoilPh: data["ph"]?.GetValue<double>() ?? 0,
            Nutrients: new SoilNutrients(
                Nitrogen: nutrientesNode?["nitrogenio"]?.GetValue<double>() ?? 0,
                Phosphorus: nutrientesNode?["fosforo"]?.GetValue<double>() ?? 0,
                Potassium: nutrientesNode?["potassio"]?.GetValue<double>() ?? 0
            )
        );
    }

    private WeatherReading ParseWeather(string id, DateTime time, Guid fieldId, JsonNode data)
    {
        return new WeatherReading(
            DeviceId: id,
            Timestamp: time,
            FieldId: fieldId,
            Temperature: data["temperatura"]?.GetValue<double>() ?? 0,
            Humidity: data["umidade"]?.GetValue<double>() ?? 0,
            RainVolume: data["chuvaUltimaHora"]?.GetValue<double>() ?? 0,
            WindSpeed: data["velocidadeVento"]?.GetValue<double>() ?? 0,
            WindDirection: data["direcaoVento"]?.ToString() ?? "N/A",
            DewPoint: data["pontoOrvalho"]?.GetValue<double>() ?? 0
        );
    }

    private SiloReading ParseSilo(string id, DateTime time, Guid fieldId, JsonNode data)
    {
        return new SiloReading(
            DeviceId: id,
            Timestamp: time,
            FieldId: fieldId,
            FillLevel: data["nivelPreenchimento"]?.GetValue<double>() ?? 0,
            Co2Level: data["co2"]?.GetValue<double>() ?? 0,
            InternalTemp: data["temperaturaMedia"]?.GetValue<double>() ?? 0
        );
    }
}