using HA.TFG.AppFinanzas.Core.Authentication;
using System.Net.Http.Json;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class UsuariosApiClient(IHttpClientFactory httpClientFactory) : IUsuarioSyncService
{
    private record EnsureRequest(
        string Email,
        string Nombre,
        string? FotoPerfil,
        bool EmailVerificado,
        DateTimeOffset? UltimaActualizacion);

    public async Task EnsureUsuarioAsync(UsuarioInfo usuario, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.PostAsJsonAsync(
            "api/usuarios/ensure",
            new EnsureRequest(
                usuario.Email,
                usuario.Nombre,
                usuario.FotoPerfil,
                usuario.EmailVerificado,
                usuario.UltimaActualizacion),
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Fallo al sincronizar usuario. Status={(int)response.StatusCode}. Body={responseBody}");
    }
}