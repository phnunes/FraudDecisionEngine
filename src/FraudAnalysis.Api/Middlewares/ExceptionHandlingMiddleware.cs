using System.Text.Json;

namespace FraudAnalysis.Api.Middlewares;

// Captura exceções não tratadas e retorna ProblemDetails (RFC 7807).
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch
        {
            context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type    = "https://tools.ietf.org/html/rfc7807",
                title   = "Erro interno no servidor",
                status  = 500,
                detail  = "Ocorreu um erro inesperado. Tente novamente mais tarde.",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}
