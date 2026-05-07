using System.ComponentModel.DataAnnotations;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;

public sealed class Auth0Config
{
    public const string SectionName = "Auth0";

    [Required(AllowEmptyStrings = false, ErrorMessage = "Auth0:Domain no está configurado.")]
    public string Domain { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Auth0:Audience no está configurado.")]
    public string Audience { get; init; } = string.Empty;
}
