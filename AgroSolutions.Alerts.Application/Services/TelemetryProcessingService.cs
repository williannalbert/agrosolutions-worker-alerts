using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.Enums;
using AgroSolutions.Alerts.Domain.Interfaces;
using AgroSolutions.Alerts.Domain.Specifications;

namespace AgroSolutions.Alerts.Application.Services;

public class TelemetryProcessingService : ITelemetryProcessingService
{
    private readonly ITelemetryParser _parser;
    private readonly ITelemetryRepository _repository;
    private readonly IHistoryIntegrationService _historyService;
    private readonly INotificationService _notificationService;

    public TelemetryProcessingService(
        ITelemetryParser parser,
        ITelemetryRepository repository,
        IHistoryIntegrationService historyService,
        INotificationService notificationService)
    {
        _parser = parser;
        _repository = repository;
        _historyService = historyService;
        _notificationService = notificationService;
    }
        
    public async Task ProcessMessageAsync(string rawJson)
    {
        try
        {
            var reading = _parser.Parse(rawJson);
            await _historyService.RegisterReadingAsync(reading);

            var alerts = new List<Alert>();

            if (reading.SoilMoisture.HasValue && reading.SoilMoisture < 30)
            {
                bool teveMomentoBom = await _historyService.HasHealthyMoistureInPeriodAsync(reading.DeviceId, 24);

                if (!teveMomentoBom)
                {
                    alerts.Add(new Alert(
                        reading.DeviceId,
                        "ALERTA DE SECA: Umidade abaixo de 30% por mais de 24h.",
                        AlertSeverity.Critical,
                        "Alerta de Seca"
                    ));
                }
            }

            var pestSpec = new PestRiskSpecification();
            if (pestSpec.IsSatisfiedBy(reading))
            {
                alerts.Add(new Alert(
                    reading.DeviceId,
                    "RISCO DE PRAGA: Condições de calor e umidade alta.",
                    AlertSeverity.Warning,
                    "Risco de Praga"
                ));
            }

            var rainSpec = new HeavyRainSpecification();
            if (rainSpec.IsSatisfiedBy(reading))
            {
                alerts.Add(new Alert(
                    reading.DeviceId,
                    "CHUVA FORTE: Acumulado > 50mm.",
                    AlertSeverity.Info,
                    "Monitoramento"
                ));
            }

            foreach (var alert in alerts)
            {
                await _repository.SaveAlertAsync(alert);
                await _notificationService.NotifyAsync(alert);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
        }
    }
}