using System.Reflection;
using FraudAnalysis.Api.Filters;
using FraudAnalysis.Api.Middlewares;
using FraudAnalysis.Application;
using FraudAnalysis.Infrastructure;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Camadas de aplicação
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------------------------------------------------------------------------
// API
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---------------------------------------------------------------------------
// Swagger / OpenAPI
// ---------------------------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FraudAnalysis API",
        Version     = "v1",
        Description = """
            ## Sobre
            Motor de decisão antifraude para transações financeiras.

            A API **recebe** transações e **delega** a análise de risco para
            um Worker assíncrono via RabbitMQ — nunca processa fraude de forma síncrona.

            ## Fluxo
            ```
            POST /transactions  →  202 Accepted  (status: Pending)
                   ↓  RabbitMQ
               Worker (RiskEngine)
                   ↓
            GET /transactions/{id}  →  200 OK  (status: Finished | decision: Approved | Rejected | Review)
            ```

            ## Idempotência
            Inclua o header `Idempotency-Key` (UUID) em toda requisição POST.
            Reenvios com a mesma chave retornam a transação original sem criar duplicata.

            ## Decisões possíveis
            | Decisão  | Significado                                      |
            |----------|--------------------------------------------------|
            | Approved | Nenhuma regra de risco acionada                  |
            | Rejected | Transação bloqueada por regra crítica            |
            | Review   | Risco moderado — requer revisão manual           |
            """,
        Contact = new OpenApiContact
        {
            Name = "Time de Engenharia — Fraud Analysis"
        }
    });

    // Operation filter: injeta Idempotency-Key como campo obrigatório nos endpoints POST
    options.OperationFilter<IdempotencyKeyOperationFilter>();

    // Inclui os comentários XML gerados pelo compilador no Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ---------------------------------------------------------------------------
// Logs estruturados
// ---------------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline HTTP
// ---------------------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── Observabilidade: métricas Prometheus ─────────────────────────────────────
// Expõe métricas HTTP automáticas (requests, latência, status codes)
app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "FraudAnalysis API v1");
        ui.RoutePrefix      = string.Empty; // Swagger na raiz: http://localhost:{port}/
        ui.DocumentTitle    = "FraudAnalysis API";
        ui.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.MapControllers();

// Endpoint /metrics para scraping do Prometheus
app.MapMetrics();

app.Run();
