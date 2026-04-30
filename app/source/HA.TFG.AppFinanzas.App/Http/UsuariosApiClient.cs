using HA.TFG.AppFinanzas.Core.Authentication;
using System.Net.Http.Json;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class UsuariosApiClient(IHttpClientFactory httpClientFactory) : IUsuarioSyncService
{
    private record SyncRequest(
        string Email,
        string Nombre,
        string? FotoPerfil,
        string? Proveedor,
        bool EmailVerificado,
        DateTimeOffset? UltimaActualizacion);

    public async Task SyncUsuarioAsync(UsuarioInfo usuario, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.PostAsJsonAsync(
            "api/usuarios/sync",
            new SyncRequest(
                usuario.Email,
                usuario.Nombre,
                usuario.FotoPerfil,
                usuario.Proveedor,
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