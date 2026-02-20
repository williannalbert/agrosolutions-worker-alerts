namespace AgroSolutions.Alerts.Application.Interfaces;

public interface IMessageConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}