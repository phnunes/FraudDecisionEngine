using System.Text.Json;

namespace FraudAnalysis.Api.Middlewares;

/// <summary>
/// Captura exceções não tratadas e retorna um ProblemDetails padronizado (RFC 7807).
/// Evita que stack traces vazem para o cliente em produção.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exceção não tratada na requisição {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type     = "https://tools.ietf.org/html/rfc7807",
                title    = "Erro interno no servidor",
                status   = 500,
                detail   = "Ocorreu um erro inesperado. Tente novamente mais tarde.",
                traceId  = context.TraceIdentifier
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}
