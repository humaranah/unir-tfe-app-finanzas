using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Diagnostics;

internal sealed class AzureExternalServicesStartupProbe(
    HealthCheckService healthCheckService,
    ILogger<AzureExternalServicesStartupProbe> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var report = await healthCheckService.CheckHealthAsync(
                registration => registration.Tags.Contains("startup-probe"),
                stoppingToken);

            foreach (var entry in report.Entries)
            {
                if (entry.Value.Status == HealthStatus.Healthy)
                {
                    logger.LogInformation("[StartupProbe] {Service}: {Description}", entry.Key, entry.Value.Description);
                }
                else if (entry.Value.Status == HealthStatus.Degraded)
                {
                    logger.LogWarning("[StartupProbe] {Service}: {Description}", entry.Key, entry.Value.Description);
                }
                else
                {
                    logger.LogError(entry.Value.Exception,
                        "[StartupProbe] {Service}: {Description}",
                        entry.Key,
                        entry.Value.Description);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[StartupProbe] Error ejecutando validaciones de conectividad de servicios Azure.");
        }
    }
}
