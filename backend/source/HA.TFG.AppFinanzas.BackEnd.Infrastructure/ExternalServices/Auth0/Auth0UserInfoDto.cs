using System.Text.Json.Serialization;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;

internal sealed class Auth0UserInfoDto
{
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("picture")]
    public string? Picture { get; init; }

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
