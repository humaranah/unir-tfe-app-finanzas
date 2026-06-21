using HA.TFG.AppFinanzas.BackEnd.Application;
using HA.TFG.AppFinanzas.BackEnd.Extensions;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using HA.TFG.AppFinanzas.BackEnd.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Azure;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando la aplicación");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        var appInsightsConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            var telemetryConfig = new TelemetryConfiguration
            {
                ConnectionString = appInsightsConnectionString
            };
            configuration.WriteTo.ApplicationInsights(telemetryConfig, TelemetryConverter.Traces);
        }
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        // Límite global conservador a nivel servidor; la política de negocio
        // por endpoint se aplica en los validadores de Application.
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    });

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddMediator(options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
    });
    builder.Services.AddAuth0(builder.Environment);
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddOpenApiWithAuth0();

    var app = builder.Build();

    // Diagnóstico de configuración de servicios Azure al arrancar (solo en desarrollo)
    if (app.Environment.IsDevelopment())
        app.LogAzureServicesConfig();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        };
    });
    app.UseGlobalExceptionHandler();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ResolveUsuarioMiddleware>();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready")
    });
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
        app.MapOpenApiAndScalar();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
