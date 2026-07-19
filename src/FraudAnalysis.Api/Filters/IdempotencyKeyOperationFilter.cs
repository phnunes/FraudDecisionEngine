using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FraudAnalysis.Api.Filters;

/// <summary>
/// Adiciona o campo Idempotency-Key como header obrigatório
/// em todos os endpoints HTTP POST da API.
/// </summary>
public class IdempotencyKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isPost = context.ApiDescription.HttpMethod?
            .Equals("POST", StringComparison.OrdinalIgnoreCase) ?? false;

        if (!isPost)
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name        = "Idempotency-Key",
            In          = ParameterLocation.Header,
            Required    = true,
            Description = "UUID único por requisição (ex: `550e8400-e29b-41d4-a716-446655440000`). " +
                          "Reenvios com a mesma chave retornam a transação original sem criar duplicata.",
            Schema = new OpenApiSchema
            {
                Type    = "string",
                Format  = "uuid",
                Example = new Microsoft.OpenApi.Any.OpenApiString(
                    Guid.NewGuid().ToString())
            }
        });
    }
}
