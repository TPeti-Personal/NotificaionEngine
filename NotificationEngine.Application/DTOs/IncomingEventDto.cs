namespace NotificationEngine.Application.DTOs;
public record IncomingEventDto(string Topic, string Asset, decimal ChangePercent, string Title);