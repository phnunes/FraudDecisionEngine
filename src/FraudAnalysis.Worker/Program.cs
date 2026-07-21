using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure;
using FraudAnalysis.Infrastructure.Messaging;
using FraudAnalysis.Worker.Consumers;
using FraudAnalysis.Worker.Engine;
using FraudAnalysis.Worker.Rules;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.AddScoped<IRiskRule, HighValueRule>();
builder.Services.AddScoped<IRiskRule, FrequencyRule>();
builder.Services.AddScoped<IRiskRule, PrivateIpRule>();
builder.Services.AddScoped<IRiskRule, ImpossibleTravelRule>();
builder.Services.AddScoped<IRiskRule, OffHoursRule>();

builder.Services.AddScoped<RiskEngine>();

builder.Services.AddHostedService<RabbitMqConsumer>();

var host = builder.Build();
host.Run();
