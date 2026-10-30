using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using NotificationEngine.Application.Interfaces;
using NotificationEngine.Infrastructure.Persistence;
using NotificationEngine.Infrastructure.Repositories;
using Core.Notification.Rule.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddSingleton<IConnection>(sp => {
    var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
    return factory.CreateConnection();
});

builder.Services.AddScoped<IAlertRuleMatcherRepository, PostgresAlertRuleMatcherRepository>();
builder.Services.AddHostedService<CoreRuleEngineWorker>();

var app = builder.Build();
app.Run();