using HA.TFG.AppFinanzas.Core.Features.Authentication;

namespace HA.TFG.AppFinanzas.Infrastructure.Clients;

public sealed class BackendHealthClient(IHttpClientFactory httpClientFactory) : IBackendHealthService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientNames.BackendHealth);
            using var response = await client.GetAsync("health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
