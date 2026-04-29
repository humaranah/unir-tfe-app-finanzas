namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface ISessionStore
{
    Task SaveRefreshTokenAsync(string refreshToken);
    Task<string?> LoadRefreshTokenAsync();
    Task SaveAccessTokenAsync(string accessToken);
    Task<string?> LoadAccessTokenAsync();
    Task ClearAsync();
}
