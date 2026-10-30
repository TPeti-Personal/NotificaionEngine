namespace NotificationEngine.Application;

public class MatchedRuleResult
{
    public Guid UserId { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelConfigJson { get; set; } = "{}";
    public string CredentialsJson { get; set; } = "{}";
}

public interface IAlertRuleMatcherRepository
{
    Task<IEnumerable<MatchedRuleResult>> GetMatchingRulesAsync(string topicPattern, string asset, decimal value);
}
