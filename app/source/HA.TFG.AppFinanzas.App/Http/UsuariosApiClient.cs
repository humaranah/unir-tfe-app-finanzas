using HA.TFG.AppFinanzas.Core.Authentication;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class UsuariosApiClient(IHttpClientFactory httpClientFactory) : IUsuarioEnsureService
{
    public async Task EnsureUsuarioAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.PostAsync("api/usuarios/ensure", null, cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Fallo al verificar usuario. Status={(int)response.StatusCode}. Body={responseBody}");
    }
}
