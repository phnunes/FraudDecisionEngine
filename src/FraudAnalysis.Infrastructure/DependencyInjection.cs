using FraudAnalysis.Domain.Interfaces;
using FraudAnalysis.Infrastructure.Context;
using FraudAnalysis.Infrastructure.Messaging;
using FraudAnalysis.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FraudAnalysis.Infrastructure;

// Registro dos serviços de infraestrutura (Postgres + RabbitMQ) no DI container.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' não encontrada.");

        services.AddDbContext<FraudDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.Configure<RabbitMqSettings>(
            configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
