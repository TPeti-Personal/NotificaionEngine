using NotificationEngine.Application.DTOs;

namespace NotificationEngine.Application.Interfaces;
public interface IAlertRuleMatcherRepository
{
    Task<IEnumerable<MatchedRuleResult>> GetMatchingRulesAsync(string topicPattern, string asset, decimal value);
}