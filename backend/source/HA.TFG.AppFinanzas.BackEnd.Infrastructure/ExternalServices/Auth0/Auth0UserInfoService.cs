using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;

public sealed class Auth0UserInfoService(HttpClient httpClient) : IAuth0UserInfoService
{
    public async Task<Auth0UserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var dto = JsonSerializer.Deserialize<Auth0UserInfoDto>(content)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta de Auth0 /userinfo.");

        return new Auth0UserInfo(
            Email: dto.Email ?? throw new InvalidOperationException("Auth0 /userinfo no devolvió 'email'."),
            Nombre: dto.Name ?? dto.Email,
            FotoPerfil: dto.Picture,
            EmailVerificado: dto.EmailVerified,
            UltimaActualizacion: dto.UpdatedAt);
    }
}
