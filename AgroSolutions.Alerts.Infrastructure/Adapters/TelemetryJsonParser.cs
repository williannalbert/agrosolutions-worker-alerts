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

        if (node["Type"]?.ToString() == "Notification" && node["Message"] != null)
        {
            var snsMessage = node["Message"]!.ToString();
            node = JsonNode.Parse(snsMessage) ?? throw new ArgumentException("Payload interno do SNS inválido");
        }

        var fieldId = (node["fieldId"] ?? node["FieldId"])?.GetValue<Guid>() ?? Guid.Empty;
        string deviceId = (node["sensorId"] ?? node["SensorId"])?.ToString() ?? "Unknown";
        DateTime timestamp = (node["timeStamp"] ?? node["TimeStamp"])?.GetValue<DateTime>() ?? DateTime.UtcNow;
        string email = (node["email"] ?? node["Email"])?.ToString() ?? "admin@agrosolutions.com";

        var typeSensorRaw = (node["typeSensor"] ?? node["TypeSensor"])?.ToString();
        var data = node["data"] ?? node["Data"];

        if (data == null) throw new ArgumentException("JSON inválido: propriedade 'data' ausente.");

        string typeSensor = MapSensorType(typeSensorRaw);

        return typeSensor switch
        {
            "solo" => ParseSoil(deviceId, timestamp, fieldId, email, data),
            "meteorologica" => ParseWeather(deviceId, timestamp, fieldId, email, data),
            "silo" => ParseSilo(deviceId, timestamp, fieldId, email, data),
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
                1 => "solo",
                2 => "silo",
                3 => "meteorologica",
                _ => "unknown"
            };
        }

        if (normalized.Contains("solo")) return "solo";
        if (normalized.Contains("meteorologica") || normalized.Contains("clima")) return "meteorologica";
        if (normalized.Contains("silo")) return "silo";

        return normalized;
    }

    private SoilReading ParseSoil(string id, DateTime time, Guid fieldId, string email, JsonNode data)
    {
        var nutrientesNode = data["nutrientesData"] ?? data["NutrientesData"]; 

        return new SoilReading(
            DeviceId: id,
            Timestamp: time,
            FieldId: fieldId,
            Email: email,
            SoilMoisture: (data["umidade"] ?? data["Umidade"])?.GetValue<double>() ?? 0,
            SoilPh: (data["ph"] ?? data["Ph"])?.GetValue<double>() ?? 0,
            Nutrients: new SoilNutrients(
                Nitrogen: (nutrientesNode?["nitrogenio"] ?? nutrientesNode?["Nitrogenio"])?.GetValue<double>() ?? 0,
                Phosphorus: (nutrientesNode?["fosforo"] ?? nutrientesNode?["Fosforo"])?.GetValue<double>() ?? 0,
                Potassium: (nutrientesNode?["potassio"] ?? nutrientesNode?["Potassio"])?.GetValue<double>() ?? 0
            )
        );
    }

    private WeatherReading ParseWeather(string id, DateTime time, Guid fieldId, string email, JsonNode data)
    {
        return new WeatherReading(
            DeviceId: id,
            Timestamp: time,
            FieldId: fieldId,
            Email: email,
            Temperature: (data["temperatura"] ?? data["Temperatura"])?.GetValue<double>() ?? 0,
            Humidity: (data["umidade"] ?? data["Umidade"])?.GetValue<double>() ?? 0,
            RainVolume: (data["chuvaUltimaHora"] ?? data["ChuvaUltimaHora"])?.GetValue<double>() ?? 0,
            WindSpeed: (data["velocidadeVento"] ?? data["VelocidadeVento"])?.GetValue<double>() ?? 0,
            WindDirection: (data["direcaoVento"] ?? data["DirecaoVento"])?.ToString() ?? "N/A",
            DewPoint: (data["pontoOrvalho"] ?? data["PontoOrvalho"])?.GetValue<double>() ?? 0
        );
    }

    private SiloReading ParseSilo(string id, DateTime time, Guid fieldId, string email, JsonNode data)
    {
        return new SiloReading(
            DeviceId: id,
            Timestamp: time,
            FieldId: fieldId,
            Email: email,
            FillLevel: (data["nivelPreenchimento"] ?? data["NivelPreenchimento"])?.GetValue<double>() ?? 0,
            Co2Level: (data["co2"] ?? data["Co2"])?.GetValue<double>() ?? 0,
            InternalTemp: (data["temperaturaMedia"] ?? data["TemperaturaMedia"])?.GetValue<double>() ?? 0
        );
    }
}