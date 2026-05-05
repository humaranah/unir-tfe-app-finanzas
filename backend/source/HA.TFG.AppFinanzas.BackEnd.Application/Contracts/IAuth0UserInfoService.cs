namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IAuth0UserInfoService
{
    Task<Auth0UserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken);
}

public record Auth0UserInfo(
    string Email,
    string Nombre,
    string? FotoPerfil,
    bool EmailVerificado,
    DateTimeOffset? UltimaActualizacion);
