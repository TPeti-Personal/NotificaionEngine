namespace NotificationEngine.Application.DTOs;
public record DispatchMessageDto(Guid UserId, string ChannelType, string ChannelConfigJson, string CredentialsJson, object Payload);