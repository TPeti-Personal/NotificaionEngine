namespace NotificationEngine.Application.DTOs;
public record MatchedRuleResult
{
    public Guid UserId { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelConfigJson { get; set; } = "{}";
    public string CredentialsJson { get; set; } = "{}";
}