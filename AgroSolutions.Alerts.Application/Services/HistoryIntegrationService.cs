using AgroSolutions.Alerts.Application.DTOs.Integration;
using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<bool> HasHealthyMoistureInPeriodAsync(string deviceId, int hours)
    {
        var startDate = DateTime.UtcNow.AddHours(-hours).ToString("yyyy-MM-ddTHH:mm:ssZ");

        return false;
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
