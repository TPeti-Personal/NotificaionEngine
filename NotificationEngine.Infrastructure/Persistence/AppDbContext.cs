using Microsoft.EntityFrameworkCore;
using NotificationEngine.Core.Models;

namespace NotificationEngine.Infrastructure.Persistence;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<UserIntegration> UserIntegrations => Set<UserIntegration>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertChannel> AlertChannels => Set<AlertChannel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.OwnsOne(e => e.Conditions, c => { c.ToJson(); });
            entity.HasIndex(e => new { e.TopicPattern, e.IsEnabled });
        });
    }
}