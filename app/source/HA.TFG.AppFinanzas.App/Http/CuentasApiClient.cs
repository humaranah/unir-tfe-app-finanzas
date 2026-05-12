using HA.TFG.AppFinanzas.Core.Cuentas;
using System.Net.Http.Json;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class CuentasApiClient(IHttpClientFactory httpClientFactory) : ICuentasService
{
    private record CreateCuentaRequest(string Moneda, string Descripcion);

    public async Task<bool> TieneCuentasAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.GetAsync("api/cuentas", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al consultar cuentas. Status={(int)response.StatusCode}. Body={body}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array
            && doc.RootElement.GetArrayLength() > 0;
    }

    public async Task CrearCuentaAsync(string descripcion, string moneda, CancellationToken cancellationToken = default)
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
