namespace NotificationEngine.Core.Models;
public class AlertConditions
{
    public string Asset { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public decimal Value { get; set; }
}