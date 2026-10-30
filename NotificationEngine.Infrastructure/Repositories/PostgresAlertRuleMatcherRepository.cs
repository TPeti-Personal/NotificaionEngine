using Microsoft.EntityFrameworkCore;
using NotificationEngine.Application.DTOs;
using NotificationEngine.Application.Interfaces;
using NotificationEngine.Infrastructure.Persistence;

namespace NotificationEngine.Infrastructure.Repositories;
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