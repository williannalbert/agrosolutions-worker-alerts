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

        var data = node["data"];
        if (data == null) throw new ArgumentException("JSON inválido: propriedade 'data' ausente.");

        string deviceId = node["sensor_id"]?.ToString() ?? "Unknown";
        DateTime timestamp = node["time_stamp"]?.GetValue<DateTime>() ?? DateTime.UtcNow;
        string? typeSensor = node["type_sensor"]?.ToString();

        return typeSensor switch
        {
            "solo" => ParseSoil(deviceId, timestamp, data),
            "meteorologica" => ParseWeather(deviceId, timestamp, data),
            "silo" => ParseSilo(deviceId, timestamp, data),
            _ => throw new NotSupportedException($"Tipo de sensor desconhecido: {typeSensor}")
        };
    }
    private SoilReading ParseSoil(string id, DateTime time, JsonNode data)
    {
        var nutrients = data["nutrients"];
        return new SoilReading(
            id, time,
            SoilMoisture: data["soil_moisture_percent"]?.GetValue<double>() ?? 0,
            SoilPh: data["soil_ph"]?.GetValue<double>() ?? 0,
            Nutrients: new SoilNutrients(
                Nitrogen: nutrients?["nitrogen_mg_kg"]?.GetValue<double>() ?? 0,
                Phosphorus: nutrients?["phosphorus_mg_kg"]?.GetValue<double>() ?? 0,
                Potassium: nutrients?["potassium_mg_kg"]?.GetValue<double>() ?? 0
            )
        );
    }

    private WeatherReading ParseWeather(string id, DateTime time, JsonNode data)
    {
        return new WeatherReading(
            id, time,
            Temperature: data["temp_celsius"]?.GetValue<double>() ?? 0,
            Humidity: data["humidity_percent"]?.GetValue<double>() ?? 0,
            RainVolume: data["rain_mm_last_hour"]?.GetValue<double>() ?? 0,
            WindSpeed: data["wind_speed_kmh"]?.GetValue<double>() ?? 0
        );
    }

    private SiloReading ParseSilo(string id, DateTime time, JsonNode data)
    {
        return new SiloReading(
            id, time,
            FillLevel: data["fill_level_percent"]?.GetValue<double>() ?? 0,
            Co2Level: data["co2_ppm"]?.GetValue<double>() ?? 0,
            InternalTemp: data["avg_temp_celsius"]?.GetValue<double>() ?? 0
        );
    }
}