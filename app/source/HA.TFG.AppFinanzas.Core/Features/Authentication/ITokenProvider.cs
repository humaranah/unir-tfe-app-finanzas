namespace HA.TFG.AppFinanzas.Core.Features.Authentication;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
