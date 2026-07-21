using FraudAnalysis.Application.Interfaces;
using FraudAnalysis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FraudAnalysis.Application;

// Registro dos serviços da camada Application no DI container.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }
}
