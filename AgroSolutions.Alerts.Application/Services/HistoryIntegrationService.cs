using AgroSolutions.Alerts.Application.DTOs.Integration;
using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AgroSolutions.Alerts.Application.Services;

public class HistoryIntegrationService : IHistoryIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    public HistoryIntegrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    public async Task RegisterReadingAsync(TelemetryReading reading)
    {
        object payloadData = MapDataPayload(reading);

        string typeSensor = payloadData switch
        {
            SoilDataDto => "solo",
            WeatherDataDto => "meteorologica",
            SiloDataDto => "silo",
            _ => "desconhecido"
        };

        var dto = new CreateReadingDto(
            FieldId: Guid.NewGuid(), // TODO: Ajustar conforme necessidade
            SensorId: Guid.TryParse(reading.DeviceId, out var id) ? id : Guid.NewGuid(),
            TypeSensor: typeSensor,
            TimeStamp: reading.Timestamp,
            Data: payloadData
        );

        var response = await _httpClient.PostAsJsonAsync("api/History", dto, _jsonOptions);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<TelemetryReading>> GetHistoryAsync(string deviceId, TimeSpan period)
    {
        var startDate = DateTime.UtcNow.Subtract(period).ToString("yyyy-MM-ddTHH:mm:ssZ");

        var url = $"api/History?sensor_id={deviceId}&start_date={startDate}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<TelemetryReading>();

            var content = await response.Content.ReadAsStringAsync();
            var jsonNodes = JsonNode.Parse(content)?.AsArray();

            if (jsonNodes == null) return Enumerable.Empty<TelemetryReading>();

            var history = new List<TelemetryReading>();

            foreach (var item in jsonNodes)
            {
                var data = item["data"];
                if (data == null) continue;

                history.Add(new TelemetryReading(
                    DeviceId: item["sensorId"]?.ToString() ?? deviceId, 
                    Timestamp: item["timestamp"]?.GetValue<DateTime>() ?? DateTime.UtcNow,
                    SoilMoisture: data["soilMoisturePercent"]?.GetValue<double>(), 
                    Temperature: data["tempCelsius"]?.GetValue<double>() ?? data["internalTempCelsius"]?.GetValue<double>(),
                    Humidity: data["humidityPercent"]?.GetValue<double>(),
                    RainVolume: data["rainMmLastHour"]?.GetValue<double>()
                ));
            }

            return history;
        }
        catch
        {
            return Enumerable.Empty<TelemetryReading>();
        }
    }

    private object MapDataPayload(TelemetryReading r)
    {
        if (r.RainVolume.HasValue || r.Temperature.HasValue && r.Humidity.HasValue)
        {
            return new WeatherDataDto(
                TempCelsius: r.Temperature ?? 0,
                HumidityPercent: r.Humidity ?? 0,
                WindSpeedKmh: 0, 
                WindDirection: "N/A",
                RainMmLastHour: r.RainVolume ?? 0,
                DewPoint: 0
            );
        }

        return new SoilDataDto(
            SoilMoisturePercent: r.SoilMoisture ?? 0,
            SoilPh: 6.5,
            Nutrients: new SoilNutrientsDto(0, 0, 0)
        );
    }
}
