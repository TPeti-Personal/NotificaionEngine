using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationEngine.Application;

namespace Core.Notification.Rule.Service;

public record IncomingEventDto(string Topic, string Asset, decimal ChangePercent, string Title);
public record DispatchMessageDto(Guid UserId, string ChannelType, string ChannelConfigJson, string CredentialsJson, object Payload);

public class CoreRuleEngineWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _rabbitConnection;
    private readonly ILogger<CoreRuleEngineWorker> _logger;
    private IModel? _channel;

    public CoreRuleEngineWorker(IServiceProvider serviceProvider, IConnection rabbitConnection, ILogger<CoreRuleEngineWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _rabbitConnection = rabbitConnection;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _rabbitConnection.CreateModel();
        
        _channel.ExchangeDeclare("event-exchange", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("core-rules-queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("core-rules-queue", "event-exchange", "market.#");
        _channel.QueueBind("core-rules-queue", "event-exchange", "news.#");
        
        _channel.ExchangeDeclare("dispatch-exchange", ExchangeType.Direct, durable: true);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(body);
                var incomingEvent = JsonSerializer.Deserialize<IncomingEventDto>(messageString);

                if (incomingEvent != null)
                {
                    _logger.LogInformation("Processing event: {Topic} for asset {Asset}", ea.RoutingKey, incomingEvent.Asset);

                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IAlertRuleMatcherRepository>();

                    var matches = await repo.GetMatchingRulesAsync(ea.RoutingKey, incomingEvent.Asset, incomingEvent.ChangePercent);

                    foreach (var match in matches)
                    {
                        var dispatchMessage = new DispatchMessageDto(
                            match.UserId,
                            match.ChannelType,
                            match.ChannelConfigJson,
                            match.CredentialsJson,
                            new { Title = incomingEvent.Title, Value = incomingEvent.ChangePercent }
                        );

                        var outputBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dispatchMessage));
                        
                        _channel.BasicPublish(
                            exchange: "dispatch-exchange",
                            routingKey: $"dispatch.{match.ChannelType}",
                            basicProperties: null,
                            body: outputBody
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue");
            }
            finally
            {
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        _channel.BasicConsume(queue: "core-rules-queue", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
