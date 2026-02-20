using AgroSolutions.Alerts.Domain.Entities;

namespace AgroSolutions.Alerts.Application.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(Alert alert);
}