using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using AgroSolutions.Alerts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Alerts.Infrastructure.Repositories;

public class AlertRepository : ITelemetryRepository
{
    private readonly AgroContext _context;

    public AlertRepository(AgroContext context)
    {
        _context = context;
    }
    public async Task SaveAlertAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsRecentAlertAsync(string deviceId, string messageStart, TimeSpan period)
    {
        var cutoff = DateTime.UtcNow.Subtract(period);

        return await _context.Alerts
            .AnyAsync(a => a.DeviceId == deviceId
                           && a.GeneratedAt >= cutoff
                           && a.Message.StartsWith(messageStart));
    }
}