using AgroSolutions.Alerts.Application.DTOs.Integration;
using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AgroSolutions.Alerts.Application.Services;

public class HistoryIntegrationService : IHistoryIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<HistoryIntegrationService> _logger;
    public HistoryIntegrationService(HttpClient httpClient, ILogger<HistoryIntegrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
        _logger = logger;
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
            FieldId: reading.FieldId,
            SensorId: Guid.Parse(reading.DeviceId),
            TypeSensor: typeSensor,
            TimeStamp: reading.Timestamp,
            Data: payloadData
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/History", dto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Histórico enviado com sucesso para API. Sensor: {SensorId}", reading.DeviceId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API retornou erro ao salvar histórico. Status: {Status}, Detalhe: {Content}",
                    response.StatusCode, errorContent);

                response.EnsureSuccessStatusCode(); 
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de conexão ao tentar salvar histórico na API.");
            throw;
        }
    }

    public async Task<IEnumerable<TelemetryReading>> GetHistoryAsync(string deviceId, TimeSpan period)
    {
        var startDate = DateTime.UtcNow.Subtract(period).ToString("yyyy-MM-ddTHH:mm:ssZ");

        var url = $"api/History?sensor_id={deviceId}&start_date={startDate}";
        _logger.LogDebug("Buscando histórico na API para {DeviceId} nas últimas {Hours} horas.", deviceId, period.TotalHours);
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao buscar histórico. Status: {Status}", response.StatusCode);
                return Enumerable.Empty<TelemetryReading>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonNodes = JsonNode.Parse(content)?.AsArray();

            if (jsonNodes == null) return Enumerable.Empty<TelemetryReading>();

            var history = new List<TelemetryReading>();

            foreach (var item in jsonNodes)
            {
                var data = item["data"];
                if (data == null) continue;

                string id = item["sensorId"]?.ToString() ?? deviceId;
                DateTime time = item["timestamp"]?.GetValue<DateTime>() ?? DateTime.UtcNow;
                Guid fieldId = item["fieldId"]?.GetValue<Guid>() ?? Guid.Empty;

                if (data["soilMoisturePercent"] != null)
                {
                    history.Add(new SoilReading(
                        id, time, fieldId, Email: "",
                        SoilMoisture: data["soilMoisturePercent"]?.GetValue<double>() ?? 0,
                        SoilPh: data["soilPh"]?.GetValue<double>() ?? 0,
                        Nutrients: null 
                    ));
                }
                else if (data["rainMmLastHour"] != null || data["windSpeedKmh"] != null)
                {
                    history.Add(new WeatherReading(
                        id, time, fieldId, Email: "",
                        Temperature: data["tempCelsius"]?.GetValue<double>() ?? 0,
                        Humidity: data["humidityPercent"]?.GetValue<double>() ?? 0,
                        RainVolume: data["rainMmLastHour"]?.GetValue<double>() ?? 0,
                        WindSpeed: data["windSpeedKmh"]?.GetValue<double>() ?? 0,
                        WindDirection: data["windDirection"]?.ToString() ?? "N/A",
                        DewPoint: data["dewPoint"]?.GetValue<double>() ?? 0));
                }
                else if (data["co2Ppm"] != null)
                {
                    history.Add(new SiloReading(
                        id, time, fieldId, Email: "",
                        FillLevel: data["fillLevelPercent"]?.GetValue<double>() ?? 0,
                        Co2Level: data["co2Ppm"]?.GetValue<double>() ?? 0,
                        InternalTemp: data["avgTempCelsius"]?.GetValue<double>() ?? 0
                    ));
                }
            }
            _logger.LogDebug("Histórico recuperado: {Count} registros.", history.Count);
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar histórico na API.");
            return Enumerable.Empty<TelemetryReading>();
        }
    }

    private object MapDataPayload(TelemetryReading r)
    {
        return r switch
        {
            SoilReading s => new SoilDataDto(
                SoilMoisturePercent: s.SoilMoisture,
                SoilPh: s.SoilPh,
                Nutrients: new SoilNutrientsDto(
                    s.Nutrients.Nitrogen,
                    s.Nutrients.Phosphorus,
                    s.Nutrients.Potassium)
            ),

            WeatherReading w => new WeatherDataDto(
                TempCelsius: w.Temperature,
                HumidityPercent: w.Humidity,
                WindSpeedKmh: w.WindSpeed,
                WindDirection: w.WindDirection, 
                RainMmLastHour: w.RainVolume,
                DewPoint: w.DewPoint
            ),

            SiloReading si => new SiloDataDto(
                FillLevelPercent: si.FillLevel,
                AvgTempCelsius: si.InternalTemp,
                Co2Ppm: si.Co2Level
            ),

            _ => new { }
        };
    }
}
