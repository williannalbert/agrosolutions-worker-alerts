using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.Enums;
using AgroSolutions.Alerts.Domain.Interfaces;
using AgroSolutions.Alerts.Domain.Specifications;
using AgroSolutions.Alerts.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Alerts.Application.Services;

public class TelemetryProcessingService : ITelemetryProcessingService
{
    private readonly ITelemetryParser _parser;
    private readonly ITelemetryRepository _repository;
    private readonly IHistoryIntegrationService _historyService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TelemetryProcessingService> _logger;

    public TelemetryProcessingService(
        ITelemetryParser parser,
        ITelemetryRepository repository,
        IHistoryIntegrationService historyService,
        INotificationService notificationService,
        ILogger<TelemetryProcessingService> logger)
    {
        _parser = parser;
        _repository = repository;
        _historyService = historyService;
        _notificationService = notificationService;
        _logger = logger;
    }
        
    public async Task ProcessMessageAsync(string rawJson)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["ProcessingId"] = Guid.NewGuid() });

        TelemetryReading? reading = null;
        try
        {
            _logger.LogDebug("Iniciando parse da mensagem. Tamanho: {Size}", rawJson.Length);

            reading = _parser.Parse(rawJson);

            _logger.LogInformation("Leitura recebida. Sensor: {DeviceId}, Data: {Timestamp}", reading.DeviceId, reading.Timestamp);
            try
            {
                await _historyService.RegisterReadingAsync(reading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar histórico para API.");
            }

            var alerts = new List<Alert>();

            var droughtSpec = new DroughtRiskSpecification();
            switch (reading)
            {
                case SoilReading soil:
                    await ProcessSoilLogic(soil, alerts);
                    break;

                case WeatherReading weather:
                    await ProcessWeatherLogic(weather, alerts);
                    break;

                case SiloReading silo:
                    ProcessSiloLogic(silo, alerts);
                    break;

                default:
                    _logger.LogWarning("Tipo de leitura não suportado para análise de alertas: {Type}", reading.GetType().Name);
                    break;
            }

            foreach (var alert in alerts)
            {
                var cooldown = alert.Severity == AlertSeverity.Critical ? TimeSpan.FromHours(1) : TimeSpan.FromHours(24);

                bool jaNotificado = await _repository.ExistsRecentAlertAsync(alert.DeviceId, alert.Message.Substring(0, 10), cooldown);

                if (!jaNotificado)
                {
                    await _repository.SaveAlertAsync(alert);
                    _logger.LogInformation("Alerta {AlertId} salvo e notificado.", alert.Id);
                    await _notificationService.NotifyAsync(alert);
                }
                else
                {
                    _logger.LogInformation("Alerta suprimido (Anti-Spam): {Message}", alert.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro fatal no processamento. Payload: {Payload}", rawJson);
            throw;
        }
    }

    private async Task ProcessSoilLogic(SoilReading soil, List<Alert> alerts)
    {
        if (soil.SoilMoisture < 30)
        {
            _logger.LogDebug("Umidade baixa ({Moisture}%). Verificando histórico...", soil.SoilMoisture);

            var droughtSpec = new DroughtRiskSpecification();

            bool persistiu = await CheckIfConditionPersisted(
                soil.DeviceId,
                TimeSpan.FromHours(24),
                r => r is SoilReading s && droughtSpec.IsSatisfiedBy(s)
            );

            if (persistiu)
            {
                _logger.LogWarning("ALERTA CONFIRMADO: Seca persistente.");
                alerts.Add(new Alert(soil.DeviceId, "ALERTA DE SECA: Umidade < 30% por 24h.", AlertSeverity.Critical, "Irrigação"));
            }
        }

        if (soil.SoilPh < 5.0 || soil.SoilPh > 8.0)
        {
            alerts.Add(new Alert(soil.DeviceId, $"SAÚDE DO SOLO: pH anormal ({soil.SoilPh}).", AlertSeverity.Warning, "Correção"));
        }
    }

    private async Task ProcessWeatherLogic(WeatherReading weather, List<Alert> alerts)
    {
        var pestSpec = new PestRiskSpecification();

        if (weather.RainVolume > 50)
        {
            alerts.Add(new Alert(weather.DeviceId, $"CHUVA FORTE: {weather.RainVolume}mm.", AlertSeverity.Info, "Monitorar"));
        }

        if (pestSpec.IsSatisfiedBy(weather))
        {
            bool persistiu = await CheckIfConditionPersisted(
                weather.DeviceId,
                TimeSpan.FromHours(24),
                r => GetTempSafe(r) > 28 && GetHumiditySafe(r) > 80 
            );

            if (persistiu)
            {
                alerts.Add(new Alert(weather.DeviceId, "RISCO DE PRAGA: Condições favoráveis persistentes.", AlertSeverity.Warning, "Dedetizar"));
            }
        }
    }

    private void ProcessSiloLogic(SiloReading silo, List<Alert> alerts)
    {
        if (silo.Co2Level > 3000)
        {
            _logger.LogCritical("PERIGO NO SILO: CO2 {Co2}ppm.", silo.Co2Level);
            alerts.Add(new Alert(silo.DeviceId, $"PERIGO SILO: CO2 Alto ({silo.Co2Level}ppm).", AlertSeverity.Critical, "EVACUAR"));
        }
    }
    private async Task<bool> CheckIfConditionPersisted(string deviceId, TimeSpan duration, Func<TelemetryReading, bool> isBadCondition)
    {
        var history = await _historyService.GetHistoryAsync(deviceId, duration);
        if (history == null || !history.Any()) return false;
        return !history.Any(r => !isBadCondition(r)); 
    }
    private double GetTempSafe(TelemetryReading r) => r is WeatherReading w ? w.Temperature : 0;
    private double GetHumiditySafe(TelemetryReading r) => r is WeatherReading w ? w.Humidity : 0;

}