namespace NotificationEngine.Core.Models;
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