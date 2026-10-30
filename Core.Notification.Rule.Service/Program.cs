using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using NotificationEngine.Infrastructure;
using NotificationEngine.Application;
using Core.Notification.Rule.Service;

var builder = WebApplication.CreateBuilder(args);

// Add Microsoft Logging explicitly if needed, WebApplication adds it by default
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddSingleton<IConnection>(sp => {
    var config = builder.Configuration;
    var factory = new ConnectionFactory() { 
        HostName = config["RabbitMQ:HostName"] ?? "localhost",
        UserName = config["RabbitMQ:UserName"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest",
        DispatchConsumersAsync = true 
    };
    return factory.CreateConnection();
});

builder.Services.AddScoped<IAlertRuleMatcherRepository, PostgresAlertRuleMatcherRepository>();
builder.Services.AddHostedService<CoreRuleEngineWorker>();

var app = builder.Build();

app.MapGet("/", () => "Core Notification Rule Service Running running with Microsoft.Extensions.Logging...");

app.Run();
