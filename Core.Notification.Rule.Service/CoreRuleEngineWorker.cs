using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationEngine.Application.DTOs;
using NotificationEngine.Application.Interfaces;

namespace Core.Notification.Rule.Service;
public class CoreRuleEngineWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _rabbitConnection;
    private readonly ILogger<CoreRuleEngineWorker> _logger;
    private readonly IModel _channel;

    public CoreRuleEngineWorker(IServiceProvider serviceProvider, IConnection rabbitConnection, ILogger<CoreRuleEngineWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _rabbitConnection = rabbitConnection;
        _logger = logger;
        _channel = _rabbitConnection.CreateModel();
        
        _channel.ExchangeDeclare("event-exchange", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("core-rules-queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("core-rules-queue", "event-exchange", "market.#");
        _channel.QueueBind("core-rules-queue", "event-exchange", "news.#");
        
        _channel.ExchangeDeclare("dispatch-exchange", ExchangeType.Direct, durable: true);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                        _channel.BasicPublish("dispatch-exchange", $"dispatch.{match.ChannelType}", null, outputBody);
                    }
                }
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume("core-rules-queue", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }
}