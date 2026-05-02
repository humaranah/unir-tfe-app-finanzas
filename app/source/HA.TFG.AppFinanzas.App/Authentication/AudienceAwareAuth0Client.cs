using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Duende.IdentityModel.OidcClient.Results;

namespace HA.TFG.AppFinanzas.App.Authentication;

internal sealed class AudienceAwareAuth0Client(IAuth0Client innerClient, string audience) : IAuth0Client
{
    private readonly IAuth0Client _innerClient = innerClient;
    private readonly string _audience = audience;

    public Task<LoginResult> LoginAsync(object? extraParameters = null, CancellationToken cancellationToken = default) =>
        _innerClient.LoginAsync(BuildExtraParameters(extraParameters), cancellationToken);

    public Task<BrowserResultType> LogoutAsync(bool federated = false, object? extraParameters = null,
        CancellationToken cancellationToken = default) =>
        _innerClient.LogoutAsync(federated, extraParameters, cancellationToken);

    public Task<AuthorizeState> PrepareLoginAsync(object? extraParameters = null,
        CancellationToken cancellationToken = default) =>
        _innerClient.PrepareLoginAsync(BuildExtraParameters(extraParameters), cancellationToken);

    public Task<LoginResult> ProcessResponseAsync(string data, AuthorizeState state, object? extraParameters = null,
        CancellationToken cancellationToken = default) =>
        _innerClient.ProcessResponseAsync(data, state, BuildExtraParameters(extraParameters), cancellationToken);

    public Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default) =>
        _innerClient.RefreshTokenAsync(refreshToken, BuildExtraParameters(null), cancellationToken);

    public Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, object? extraParameters,
        CancellationToken cancellationToken = default) =>
        _innerClient.RefreshTokenAsync(refreshToken, BuildExtraParameters(extraParameters), cancellationToken);

    public Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, string scope, object? extraParameters,
        CancellationToken cancellationToken = default) =>
        _innerClient.RefreshTokenAsync(refreshToken, scope, BuildExtraParameters(extraParameters), cancellationToken);

    public Task<UserInfoResult> GetUserInfoAsync(string accessToken) =>
        _innerClient.GetUserInfoAsync(accessToken);

    private object? BuildExtraParameters(object? extraParameters)
    {
        if (string.IsNullOrWhiteSpace(_audience))
            return extraParameters;

        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["audience"] = _audience
        };

        if (extraParameters is null)
            return merged;

        if (extraParameters is IReadOnlyDictionary<string, string> readOnlyDictionary)
        {
            foreach (var item in readOnlyDictionary)
                merged[item.Key] = item.Value;

            return merged;
        }

        if (extraParameters is IDictionary<string, string> dictionary)
        {
            foreach (var item in dictionary)
                merged[item.Key] = item.Value;

            return merged;
        }

        foreach (var property in extraParameters.GetType().GetProperties())
        {
            var value = property.GetValue(extraParameters)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                merged[property.Name] = value;
        }

        return merged;
    }
}