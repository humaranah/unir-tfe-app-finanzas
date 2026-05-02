namespace HA.TFG.AppFinanzas.Core.Authentication;

/// <summary>
/// Información del usuario obtenida del proveedor de identidad (Auth0/OIDC).
/// </summary>
public record UsuarioInfo(
    /// <summary>Correo electrónico del usuario.</summary>
    string Email,

    /// <summary>Nombre completo del usuario.</summary>
    string Nombre,

    /// <summary>URL de la foto de perfil.</summary>
    string? FotoPerfil,

    /// <summary>
    /// Proveedor de identidad extraído del claim 'sub'.
    /// Ejemplos: "google-oauth2", "github", "auth0".
    /// </summary>
    string? Proveedor,

    /// <summary>Indica si el email ha sido verificado por el proveedor.</summary>
    bool EmailVerificado,

    /// <summary>Fecha de última actualización del perfil en el proveedor.</summary>
    DateTimeOffset? UltimaActualizacion);
