using FraudAnalysis.Application.Interfaces;
using FraudAnalysis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FraudAnalysis.Application;

/// <summary>
/// Registro dos serviços da camada Application no DI container.
/// Chamado uma única vez no Program.cs da API.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }
}
