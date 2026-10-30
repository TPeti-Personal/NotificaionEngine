using System.Text.Json.Serialization;

namespace NotificationEngine.Core;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public ICollection<UserIntegration> Integrations { get; set; } = new List<UserIntegration>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}

public class UserIntegration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Provider { get; set; } = string.Empty;
    public string CredentialsJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
}

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

public class AlertConditions
{
    public string Asset { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class AlertChannel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AlertRuleId { get; set; }
    public AlertRule AlertRule { get; set; } = null!;
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelConfigJson { get; set; } = "{}";
}
