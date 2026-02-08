using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.Interfaces;
using AgroSolutions.Alerts.Domain.ValueObjects;
using AgroSolutions.Alerts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Alerts.Infrastructure.Repositories;

public class InMemoryTelemetryRepository : ITelemetryRepository
{
    private readonly AgroContext _context;

    public InMemoryTelemetryRepository(AgroContext context)
    {
        _context = context;
    }
    public async Task SaveAlertAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
    }
}