namespace NotificationEngine.Core.Models;
public class UserIntegration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Provider { get; set; } = string.Empty;
    public string CredentialsJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
}