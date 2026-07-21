using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FraudAnalysis.Api.Filters;

// Adiciona o header Idempotency-Key como obrigatório nos endpoints POST do Swagger.
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
            Description = "UUID único por requisição para garantir idempotência.",
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
