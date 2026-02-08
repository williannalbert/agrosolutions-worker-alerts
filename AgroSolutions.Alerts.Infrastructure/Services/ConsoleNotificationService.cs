using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.Enums;

namespace AgroSolutions.Alerts.Infrastructure.Services;

public class ConsoleNotificationService : INotificationService
{
    public Task NotifyAsync(Alert alert)
    {
        var color = alert.Severity switch
        {
            AlertSeverity.Critical => ConsoleColor.Red,
            AlertSeverity.Warning => ConsoleColor.Yellow,
            _ => ConsoleColor.Cyan
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"--- NOTIFICAÇÃO ENVIADA ---");
        Console.WriteLine($"Destino: Gerente da Fazenda");
        Console.WriteLine($"Assunto: {alert.Message}");
        Console.WriteLine($"Ação Sugerida: Mudar talhão para '{alert.SuggestedFieldStatus}'");
        Console.WriteLine($"---------------------------");
        Console.ResetColor();

        return Task.CompletedTask;
    }
}