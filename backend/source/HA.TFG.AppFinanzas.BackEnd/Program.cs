using HA.TFG.AppFinanzas.BackEnd.Application;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

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

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();
    builder.Services.AddMediator();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
