using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class BackendHealthClient(IHttpClientFactory httpClientFactory) : IBackendHealthService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("BackendHealth");
            using var response = await client.GetAsync("health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
