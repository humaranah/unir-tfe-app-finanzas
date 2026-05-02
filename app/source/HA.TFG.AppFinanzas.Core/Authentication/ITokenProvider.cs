namespace HA.TFG.AppFinanzas.Core.Authentication;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
