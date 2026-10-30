namespace NotificationEngine.Core.Models;
public class AlertChannel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AlertRuleId { get; set; }
    public AlertRule AlertRule { get; set; } = null!;
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelConfigJson { get; set; } = "{}";
}