using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure;
using FraudAnalysis.Infrastructure.Messaging;
using FraudAnalysis.Worker.Consumers;
using FraudAnalysis.Worker.Engine;
using FraudAnalysis.Worker.Rules;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

// ── Infraestrutura compartilhada (Postgres + repositório) ───────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── RabbitMQ settings (consumer usa IOptions<RabbitMqSettings>) ─────────────
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// ── Regras de risco ─────────────────────────────────────────────────────────
// Cada IRiskRule é registrada individualmente.
// O RiskEngine recebe IEnumerable<IRiskRule> e executa todas na ordem de registro.
builder.Services.AddScoped<IRiskRule, SuspiciousAmountRule>(); // Rejected: valor == 100
builder.Services.AddScoped<IRiskRule, HighValueRule>();        // Review:   valor > 10.000
builder.Services.AddScoped<IRiskRule, FrequencyRule>();        // Rejected: > 5 transações/min
builder.Services.AddScoped<IRiskRule, PrivateIpRule>();        // Review:   IP privado/inválido
builder.Services.AddScoped<IRiskRule, ImpossibleTravelRule>(); // Rejected: velocidade > 1000 km/h
builder.Services.AddScoped<IRiskRule, OffHoursRule>();         // Review:   00h-05h horário Brasília

// ── Motor de decisão ─────────────────────────────────────────────────────────
builder.Services.AddScoped<RiskEngine>();

// ── Consumer RabbitMQ (BackgroundService) ───────────────────────────────────
builder.Services.AddHostedService<RabbitMqConsumer>();

var host = builder.Build();

// ── Observabilidade: Prometheus MetricServer (porta 9090) ────────────────────
// O Worker não é uma web app, então expõe métricas via servidor standalone.
using var metricServer = new MetricServer(port: 9090);
metricServer.Start();

host.Run();
