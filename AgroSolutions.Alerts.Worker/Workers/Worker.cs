using AgroSolutions.Alerts.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Alerts.Worker.Workers;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AgroSmart Worker iniciado às: {time}", DateTimeOffset.Now);

        var mensagensDeTeste = ObterMensagensDeTeste();

        foreach (var jsonMessage in mensagensDeTeste)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<TelemetryProcessingService>();

                    _logger.LogInformation(">>> Recebendo nova mensagem...");

                    await processor.ProcessMessageAsync(jsonMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem.");
            }

            await Task.Delay(2000, stoppingToken);
        }

        _logger.LogInformation("Simulação finalizada.");
    }

    private List<string> ObterMensagensDeTeste()
    {
        var fieldId = Guid.NewGuid();
        var sensorSoloId = Guid.NewGuid();
        var sensorMeteoId = Guid.NewGuid();

        return new List<string>
    {
        $$"""
        {
            "field_id": "{{fieldId}}",
            "sensor_id": "{{sensorSoloId}}",
            "type_sensor": "solo", 
            "time_stamp": "2026-01-24T10:00:00Z",
            "data": {
                "soil_moisture_percent": 45.0,
                "soil_ph": 6.5,
                "nutrients": {
                    "nitrogen_mg_kg": 100,
                    "phosphorus_mg_kg": 50,
                    "potassium_mg_kg": 120
                }
            }
        }
        """,

        $$"""
        {
            "field_id": "{{fieldId}}",
            "sensor_id": "{{sensorSoloId}}",
            "type_sensor": "solo",
            "time_stamp": "2026-01-24T11:00:00Z",
            "data": {
                "soil_moisture_percent": 25.0,
                "soil_ph": 6.5,
                "nutrients": { "nitrogen_mg_kg": 100, "phosphorus_mg_kg": 50, "potassium_mg_kg": 120 }
            }
        }
        """,

        $$"""
        {
            "field_id": "{{fieldId}}",
            "sensor_id": "{{sensorSoloId}}",
            "type_sensor": "solo",
            "time_stamp": "2026-01-24T12:00:00Z",
            "data": {
                "soil_moisture_percent": 20.0,
                "soil_ph": 6.4,
                "nutrients": { "nitrogen_mg_kg": 98, "phosphorus_mg_kg": 48, "potassium_mg_kg": 118 }
            }
        }
        """,

        $$"""
        {
            "field_id": "{{fieldId}}",
            "sensor_id": "{{sensorSoloId}}",
            "type_sensor": "solo",
            "time_stamp": "2026-01-24T13:00:00Z",
            "data": {
                "soil_moisture_percent": 15.0,
                "soil_ph": 6.3,
                "nutrients": { "nitrogen_mg_kg": 95, "phosphorus_mg_kg": 45, "potassium_mg_kg": 115 }
            }
        }
        """,

        $$"""
        {
            "field_id": "{{fieldId}}",
            "sensor_id": "{{sensorMeteoId}}",
            "type_sensor": "meteorologica",
            "time_stamp": "2026-01-24T14:00:00Z",
            "data": {
                "temp_celsius": 28.5,
                "humidity_percent": 62.0,
                "rain_mm_last_hour": 60.0,
                "wind_speed_kmh": 15.0,
                "wind_direction": "SE",
                "dew_point": 21.0
            }
        }
        """
    };
    }
}