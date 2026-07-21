using System.Reflection;
using FraudAnalysis.Api.Filters;
using FraudAnalysis.Api.Middlewares;
using FraudAnalysis.Application;
using FraudAnalysis.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FraudAnalysis API",
        Version     = "v1",
        Description = "Motor de decisão antifraude para transações financeiras.",
        Contact = new OpenApiContact
        {
            Name = "Time de Engenharia — Fraud Analysis"
        }
    });

    options.OperationFilter<IdempotencyKeyOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "FraudAnalysis API v1");
        ui.RoutePrefix      = string.Empty;
        ui.DocumentTitle    = "FraudAnalysis API";
        ui.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
