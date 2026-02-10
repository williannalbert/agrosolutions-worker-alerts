using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Alerts.Worker.Workers;

public class Worker : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<Worker> _logger;
    public Worker(IMessageConsumer consumer, ILogger<Worker> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado.");

        await _consumer.StartConsumingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}