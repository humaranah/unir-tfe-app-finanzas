namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface ISessionStore
{
    Task SaveRefreshTokenAsync(string refreshToken);
    Task<string?> LoadRefreshTokenAsync();
    Task ClearAsync();
}
