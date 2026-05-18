using HA.TFG.AppFinanzas.Core.Cuentas;
using System.Net.Http.Json;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class CuentasApiClient(IHttpClientFactory httpClientFactory) : ICuentasService
{
    private record CreateCuentaRequest(string Moneda, string Descripcion);
    private record CuentaResponse(Guid Id, string Moneda, string Descripcion);

    public async Task<bool> HasCuentasAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await GetCuentasAsync(cancellationToken);
        return cuentas.Count > 0;
    }

    public async Task<(Guid? Id, string? Descripcion)> GetDefaultCuentaAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await GetCuentasAsync(cancellationToken);
        return cuentas.Count > 0 ? (cuentas[0].Id, cuentas[0].Descripcion) : (null, null);
    }

    private async Task<List<CuentaResponse>> GetCuentasAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.GetAsync("api/cuentas", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al consultar cuentas. Status={(int)response.StatusCode}. Body={body}");
        }

        return await response.Content.ReadFromJsonAsync<List<CuentaResponse>>(cancellationToken) ?? [];
    }

    public async Task CreateCuentaAsync(string descripcion, string moneda, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.PostAsJsonAsync(
            "api/cuentas",
            new CreateCuentaRequest(moneda, descripcion),
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Error al crear cuenta. Status={(int)response.StatusCode}. Body={body}");
    }
}
