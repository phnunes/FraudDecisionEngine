using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure.Context;
using FraudAnalysis.Infrastructure.Messaging;
using FraudAnalysis.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FraudAnalysis.Infrastructure;

/// <summary>
/// Registro dos serviços da camada Infrastructure no DI container.
/// Chamado uma única vez no Program.cs da API.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL ────────────────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' não encontrada. " +
                "Configure em appsettings.json ou via variável de ambiente.");

        services.AddDbContext<FraudDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // ── RabbitMQ ─────────────────────────────────────────────────────────
        services.Configure<RabbitMqSettings>(
            configuration.GetSection(RabbitMqSettings.SectionName));

        // Scoped: uma conexão por request — adequado para publicação pontual na API.
        // O IAsyncDisposable do RabbitMqPublisher garante o fechamento ao fim do escopo.
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
