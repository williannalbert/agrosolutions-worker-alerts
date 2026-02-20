using AgroSolutions.Alerts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Alerts.Infrastructure.Data;

public class AgroContext : DbContext
{
    public AgroContext(DbContextOptions<AgroContext> options) : base(options) { }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
