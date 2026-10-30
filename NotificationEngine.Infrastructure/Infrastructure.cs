using Microsoft.EntityFrameworkCore;
using NotificationEngine.Core;
using NotificationEngine.Application;

namespace NotificationEngine.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<UserIntegration> UserIntegrations => Set<UserIntegration>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertChannel> AlertChannels => Set<AlertChannel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertRule>()
            .OwnsOne(b => b.Conditions, builder => {
                builder.ToJson();
            });

        modelBuilder.Entity<AlertRule>()
            .HasIndex(b => new { b.TopicPattern, b.IsEnabled });
    }
}

public class PostgresAlertRuleMatcherRepository : IAlertRuleMatcherRepository
{
    private readonly AppDbContext _context;

    public PostgresAlertRuleMatcherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MatchedRuleResult>> GetMatchingRulesAsync(string topicPattern, string asset, decimal value)
    {
        return await _context.AlertRules
            .Where(r => r.TopicPattern == topicPattern && r.IsEnabled)
            .Where(r => r.Conditions.Asset == asset)
            .SelectMany(r => r.Channels, (rule, channel) => new { rule, channel })
            .Join(_context.UserIntegrations,
                rc => new { rc.rule.UserId, Provider = rc.channel.ChannelType },
                ui => new { ui.UserId, ui.Provider },
                (rc, ui) => new MatchedRuleResult
                {
                    UserId = rc.rule.UserId,
                    ChannelType = rc.channel.ChannelType,
                    ChannelConfigJson = rc.channel.ChannelConfigJson,
                    CredentialsJson = ui.IsActive ? ui.CredentialsJson : "{}"
                })
            .ToListAsync();
    }
}
