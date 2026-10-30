namespace NotificationEngine.Core.Models;
public class AlertRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string TopicPattern { get; set; } = string.Empty;
    public AlertConditions Conditions { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public ICollection<AlertChannel> Channels { get; set; } = new List<AlertChannel>();
}