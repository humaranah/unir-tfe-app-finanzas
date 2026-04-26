using HA.TFG.AppFinanzas.BackEnd.Application;
using HA.TFG.AppFinanzas.BackEnd.Extensions;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure;
using HA.TFG.AppFinanzas.BackEnd.Middleware;
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
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddMediator(options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
    });
    builder.Services.AddAuth0(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddOpenApiWithAuth0();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseGlobalExceptionHandler();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health");
    app.MapControllers();

    if (app.Environment.IsDevelopment())
        app.MapOpenApiAndScalar();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
